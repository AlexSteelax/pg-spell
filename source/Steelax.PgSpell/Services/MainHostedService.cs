using Microsoft.Extensions.Options;
using NJsonSchema;
using Steelax.PgSpell.Helpers;
using Steelax.PgSpell.Schema;
using Steelax.PgSpell.Settings;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Steelax.PgSpell.Services
{
    public class MainHostedService : IHostedService
    {
        readonly ILogger<MainHostedService> _logger;
        readonly IOptions<SqlGenerateSettings> _genOptions;
        readonly SemaphoreSlim _semaphore = new(Environment.ProcessorCount);
        public MainHostedService(ILogger<MainHostedService> logger, IOptions<SqlGenerateSettings> genOptions) =>
            (_logger, _genOptions) = (logger, genOptions);

        private async Task<IEnumerable<PgSpellCollection>?> LoadDescriptions()
        {
            if (string.IsNullOrEmpty(_genOptions.Value.PathIn))
                throw new ArgumentNullException("Description directory path was unsetted.");

            var yamlFiles = Directory.EnumerateFiles(_genOptions.Value.PathIn).Where(f => f.EndsWith(".yaml")).ToArray();

            if (yamlFiles.Length == 0)
            {
                _logger.LogInformation("Yaml description files not found.");
                return null;
            }
            else
            {
                var count = yamlFiles.Length;
                _logger.LogInformation("Found {count} yaml description file(s).", count);
            }

            var schemaResource = Resource.Read("pg-spell.json");
            var schema = await JsonSchema.FromJsonAsync(Encoding.UTF8.GetString(schemaResource));
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

            var result = await yamlFiles
                .Select<string, (bool Success, PgSpellCollection? Result)>(filePath =>
                {
                    var relPath = Path.GetRelativePath(_genOptions.Value.PathIn, filePath);
                    _logger.LogInformation("Processing {filePath}", relPath);
                    try
                    {
                        using var stream = File.OpenRead(filePath);
                        using var reader = new StreamReader(stream);

                        var data = deserializer.Deserialize<PgSpellCollection>(reader);
                        var errors = schema.Validate(data);

                        foreach (var error in errors)
                        {
                            var kind = error.Kind;
                            var line = error.LineNumber;
                            var pos = error.LinePosition;
                            var path = error.Path;
                            var prop = error.Property;
                            var err = error.ToString();

                            _logger.LogError("{kind}: line {line} at pos {pos} from path {path} in {prop}\n{err}", kind, line, pos, path, prop, err);
                        }

                        return (!errors.Any(), data);
                    }
                    catch(Exception e)
                    {
                        _logger.LogCritical("{e}", e);
                    }
                    return (false, null);
                })
                .Select(async action =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        return await Task.Run(() => action);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                })
                .WhenAll();

            if (result.Select(item => item.Success).Contains(false))
            {
                _logger.LogError("There are processing errors.");
                return null;
            }
            else
            {
                _logger.LogInformation("All files have been processed.");
                return result.Select(item => item.Result);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var result = await LoadDescriptions();

            if (result is null)
                return;


            var sql = new StringBuilder();

            sql.AppendLine("--Auto generated\n");

            result.SelectMany(item =>
                (item.Tables ?? Enumerable.Empty<Table>()).Select(item => item.Schema)
                .Union(
                    (item.Composites ?? Enumerable.Empty<Composite>()).Select(item => item.Schema)))
                .Distinct()
                .ToList().ForEach(s =>
                {
                    sql.AppendLine($"DROP schema IF EXISTS {s} CASCADE;");
                    sql.AppendLine($"CREATE schema IF NOT EXISTS {s};");
                });

            result.SelectMany(item => item.Composites ?? Enumerable.Empty<Composite>()).ToList().ForEach(t =>
            {
                sql.AppendLine($"--{t.Comment}");
                sql.AppendLine($"create type {t.Schema}.{t.Name} as");
                sql.AppendLine("(");
                var idx = 0;
                var clen = t.Columns.Count;
                foreach (var c in t.Columns)
                {
                    idx++;
                    var type = c.Type.TrimStart('$');
                    sql.AppendLine($"\t{c.Name} {type}{(idx == clen ? "" : ",")}");
                }
                sql.AppendLine(");");
            });

            result.SelectMany(item => item.Tables ?? Enumerable.Empty<Table>()).ToList().ForEach(t =>
            {
                sql.AppendLine($"--{t.Comment}");
                sql.AppendLine($"create table if not exists {t.Schema}.{t.Name}");
                sql.AppendLine("(");
                var idx = 0;
                var clen = t.Columns.Count;
                foreach (var c in t.Columns)
                {
                    idx++;
                    var type = c.Type.TrimStart('$');
                    sql.AppendLine($"\t{c.Name} {type}{(c.Name == "id" ? " generated always as identity primary key" : "")}{(idx == clen ? "" : ",")}");
                }
                sql.AppendLine(");");
            });

            await File.WriteAllTextAsync(Path.Combine(_genOptions.Value.PathOut, "struct.sql"), sql.ToString());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}