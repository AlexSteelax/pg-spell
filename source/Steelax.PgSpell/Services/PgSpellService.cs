using NJsonSchema;
using Steelax.PgSpell.Helpers;
using Steelax.PgSpell.Model;
using Steelax.PgSpell.Schema;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Steelax.PgSpell.Services
{
    public interface IPgSpell
    {
        void CleadDefinition();
        Task<bool> LoadDefinition(string definitionDir, SearchOption searchOption = SearchOption.AllDirectories);

        ReadOnlyCollection<SqlObject> BuildSchemaSql(BuildSqlFilter? filter = default);
        ReadOnlyCollection<SqlObject> BuildEnumSql(BuildSqlFilter? filter = default);
        ReadOnlyCollection<SqlObject> BuildCompositeSql(BuildSqlFilter? filter = default, bool withDependencies = false);
        ReadOnlyCollection<SqlObject> BuildTableSql(BuildSqlFilter? filter = default, bool withDependencies = false);

    }
    public delegate bool BuildSqlFilter(string objectName);
    public class PgSpellService: IPgSpell
    {
        private readonly ILogger<PgSpellService> _logger;

        private readonly Dictionary<string, Schema.Enum> _enums = new();
        private readonly Dictionary<string, Schema.Composite> _composites = new();
        private readonly Dictionary<string, Schema.Table> _tables = new();
        private readonly List<string> _schemas = new();

        public PgSpellService(ILogger<PgSpellService> logger) =>
            (_logger) = (logger);

        public void CleadDefinition()
        {
            _enums.Clear();
            _composites.Clear();
            _tables.Clear();
            _schemas.Clear();
        }
        public async Task<bool> LoadDefinition(string definitionDir, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (string.IsNullOrEmpty(definitionDir))
                throw new ArgumentNullException(nameof(definitionDir));

            var yamlFiles = Directory.EnumerateFiles(definitionDir, "*.yaml", searchOption).ToArray();

            if (yamlFiles.Length == 0)
            {
                _logger.LogInformation("Yaml description files not found.");
                return true;
            }
            else
            {
                var count = yamlFiles.Length;
                _logger.LogInformation("Found {count} yaml description file(s).", count);
            }
            var schemaResource = Resource.Read("pg-spell.json");
            var schema = await JsonSchema.FromJsonAsync(Encoding.UTF8.GetString(schemaResource));
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

            var success = true;
            var collection = new ConcurrentBag<PgSpellCollection>();

            await Task.Run(() => Parallel.ForEach(yamlFiles, filePath =>
            {
                var _rel = Path.GetRelativePath(definitionDir, filePath);
                _logger.LogInformation("Processing {filePath}", _rel);

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

                        _logger.LogError("{filePath} {kind}: line {line} at pos {pos} from path {path} in {prop}\n{err}", _rel, kind, line, pos, path, prop, err);
                    }

                    if (errors.Any())
                    {
                        success = false;
                    }
                    else
                    {
                        collection.Add(data);
                    }
                }
                catch (Exception e)
                {
                    success = false;
                    _logger.LogCritical("{e}", e);
                }
            }));

            if (!success)
            {
                _logger.LogError("There are processing errors.");
            }
            else
            {
                collection
                    .Select(c => c.Enums)
                    .Where(c => c is not null)
                    .SelectMany(c => c)
                    .ToList()
                    .ForEach(item =>
                    {
                        _enums.Add($"{item.Schema}.{item.Name}", item);
                    });

                collection
                    .Select(c => c.Composites)
                    .Where(c => c is not null)
                    .SelectMany(c => c)
                    .ToList()
                    .ForEach(item =>
                    {
                        _composites.Add($"{item.Schema}.{item.Name}", item);
                    });

                collection
                    .Select(c => c.Tables)
                    .Where(c => c is not null)
                    .SelectMany(c => c)
                    .ToList()
                    .ForEach(item =>
                    {
                        _tables.Add($"{item.Schema}.{item.Name}", item);
                    });

                _enums.Select(v => v.Value.Schema)
                    .Union(_composites.Select(v => v.Value.Schema))
                    .Union(_tables.Select(v => v.Value.Schema))
                    .Distinct()
                    .ToList()
                    .ForEach(item =>
                    {
                        _schemas.Add(item);
                    });

                _logger.LogInformation("All files have been processed.");
            }

            return success;
        }

        private IEnumerable<SqlObject> WalkDeep(SqlObject obj)
        {
            var stack = new Stack<SqlObject>();

            stack.Push(obj);

            while (stack.Any())
            {
                var next = stack.Pop();

                yield return next;

                if (next.DType == SqlObjectType.Enum)
                {
                    //nothing
                }
                else if (next.DType == SqlObjectType.Composite)
                {
                    var d = next.Definition as Schema.Composite;

                    if (d!.Columns is not null)
                        d.Columns.Select(c =>
                        {
                            if (_enums.TryGetValue(c.Type, out Schema.Enum? ev))
                            {
                                return new SqlObject(ev);
                            }
                            else if (_composites.TryGetValue(c.Type, out Schema.Composite? cv))
                            {
                                return new SqlObject(cv);
                            }
                            return null;
                        }).Where(v => v is not null).ToList().ForEach(item =>
                        {
                            stack.Push(item!);
                        });
                }
                else if (next.DType == SqlObjectType.Table)
                {
                    var d = next.Definition as Schema.Table;

                    if (d!.Columns is not null)
                        d.Columns.Select(c =>
                        {
                            if (_enums.TryGetValue(c.Type, out Schema.Enum? ev))
                            {
                                return new SqlObject(ev);
                            }
                            else if (_composites.TryGetValue(c.Type, out Schema.Composite? cv))
                            {
                                return new SqlObject(cv);
                            }
                            else if (_tables.TryGetValue(c.Type, out Schema.Table? tv))
                            {
                                return new SqlObject(tv);
                            }
                            return null;
                        }).Where(v => v is not null).ToList().ForEach(item =>
                        {
                            stack.Push(item!);
                        });
                }
            }
        }

        public ReadOnlyCollection<SqlObject> BuildSchemaSql(BuildSqlFilter? filter = default)
        {
            return _schemas
                .Where(v => filter is null || filter(v))
                .ToList()
                .Select(item => new SqlObject(item)).ToList().AsReadOnly();
        }
        public ReadOnlyCollection<SqlObject> BuildEnumSql(BuildSqlFilter? filter = default)
        {
            return _enums
                .Where(v => filter is null || filter(v.Key))
                .ToList()
                .Select(item => new SqlObject(item.Value)).ToList().AsReadOnly();
        }
        public ReadOnlyCollection<SqlObject> BuildCompositeSql(BuildSqlFilter? filter = default, bool withDependencies = false)
        {
            return _composites
                .Where(v => filter is null || filter(v.Key))
                .ToList()
                .SelectMany(item =>
                {
                    var o = new SqlObject(item.Value);
                    return withDependencies ? WalkDeep(o) : new SqlObject[] { o };
                }).Reverse().DistinctBy(item => item.FullName).ToList().AsReadOnly();
        }
        public ReadOnlyCollection<SqlObject> BuildTableSql(BuildSqlFilter? filter = default, bool withDependencies = false)
        {
            return _tables
                .Where(v => filter is null || filter(v.Key))
                .ToList()
                .SelectMany(item =>
                {
                    var o = new SqlObject(item.Value);
                    return withDependencies ? WalkDeep(o) : new SqlObject[] { o };
                }).Reverse().DistinctBy(item => item.FullName).ToList().AsReadOnly();
        }
    }
}