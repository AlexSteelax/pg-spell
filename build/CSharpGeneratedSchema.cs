using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System;
using System.IO;
using System.Threading.Tasks;

internal static class CSharpSchemaGenerate
{
    public static async Task Generate(string schemaFile, Func<CSharpSchemaGenerateSettings, CSharpSchemaGenerateSettings> configure)
    {
        var settings = configure(new CSharpSchemaGenerateSettings());

        settings.Validate();

        var file = await File.ReadAllTextAsync(schemaFile);
        var schema = await JsonSchema.FromJsonAsync(file);

        var classGenerator = new CSharpGenerator(schema, new CSharpGeneratorSettings
        {
            ClassStyle = CSharpClassStyle.Poco,
            Namespace = string.IsNullOrEmpty(settings.Namespace) ? "MyNamespace" : settings.Namespace
        });

        var codeFile = classGenerator.GenerateFile();

        await File.WriteAllTextAsync(Path.Combine(settings.Directory, $"{settings.FileName}.Generated.cs"), codeFile);
    }
}

public class CSharpSchemaGenerateSettings
{
    private string _fileName;
    private string _nameSpace;
    private string _dir;

    internal CSharpSchemaGenerateSettings() { }
    private CSharpSchemaGenerateSettings(string fileName, string nameSpace, string dir)
    {
        _fileName = fileName;
        _nameSpace = nameSpace;
        _dir = dir;
    }

    public string FileName => _fileName;
    public string Namespace => _nameSpace;
    public string Directory => _dir;

    public CSharpSchemaGenerateSettings SetFileName(string value) =>
        new CSharpSchemaGenerateSettings(value, _nameSpace, _dir);
    public CSharpSchemaGenerateSettings SetNamespace(string value) =>
        new CSharpSchemaGenerateSettings(_fileName, value, _dir);
    public CSharpSchemaGenerateSettings SetDirectory(string value) =>
        new CSharpSchemaGenerateSettings(_fileName, _nameSpace, value);

    internal void Validate()
    {
        if (string.IsNullOrEmpty(_fileName))
            throw new ArgumentException("Param was unsetted", nameof(SetFileName));
        if (string.IsNullOrEmpty(_nameSpace))
            throw new ArgumentException("Param was unsetted", nameof(SetNamespace));
        if (string.IsNullOrEmpty(_dir))
            throw new ArgumentException("Param was unsetted", nameof(SetDirectory));
    }
}

