using System.Text;

namespace Steelax.PgSpell.Model
{
    public enum SqlObjectType
    {
        Enum, Composite, Table, Schema
    }
    public sealed class SqlObject
    {
        public string? FullName { get; private set; }
        public SqlObjectType DType { get; private set; }
        public object Definition { get; init; }
        public SqlObject(object definition)
        {
            Definition = definition;

            if (definition is Schema.Enum ev)
            {
                DType = SqlObjectType.Enum;
                FullName = $"{ev.Schema}.{ev.Name}";
            }
            else if (definition is Schema.Composite cv)
            {
                DType = SqlObjectType.Composite;
                FullName = $"{cv.Schema}.{cv.Name}";
            }
            else if (definition is Schema.Table tv)
            {
                DType = SqlObjectType.Table;
                FullName = $"{tv.Schema}.{tv.Name}";
            }
            else if (definition is string sv)
            {
                DType = SqlObjectType.Schema;
                FullName = sv;
            }
        }

        public string ToCreateSql()
        {
            var sql = new StringBuilder();

            if (DType == SqlObjectType.Enum)
            {
                var t = Definition as Schema.Enum;

                sql.AppendLine($"CREATE TYPE {FullName} AS ENUM");
                sql.Append("\t(");
                sql.Append(string.Join(",", t.Items.Select(e => $"'{e}'")));
                sql.AppendLine(");");
                sql.AppendLine($"COMMENT ON TYPE {FullName} IS '{t.Comment}';");
            }
            else if (DType == SqlObjectType.Composite)
            {
                var t = Definition as Schema.Composite;

                sql.AppendLine($"CREATE TYPE {FullName} AS");
                sql.AppendLine("(");
                var idx = 0;
                var clen = t.Columns?.Count ?? 0;
                if (clen > 0)
                    foreach (var c in t.Columns!)
                    {
                        idx++;
                        var type = c.Type.TrimStart('$');
                        sql.AppendLine($"\t{c.Name} {type}{(idx == clen ? "" : ",")}");
                    }
                sql.AppendLine(");");
                sql.AppendLine($"COMMENT ON TYPE {FullName} IS '{t.Comment}';");
            }
            else if (DType == SqlObjectType.Table)
            {
                var t = Definition as Schema.Table;

                sql.AppendLine($"CREATE TABLE IF NOT EXISTS {FullName}");
                sql.AppendLine("(");
                if (t.Columns is not null && t.Columns.Any())
                {
                    var first = true;
                    foreach (var c in t.Columns)
                    {
                        var type = c.Type.TrimStart('$');

                        if (!first) sql.AppendLine(",");

                        sql.Append($"\t{c.Name} {type}");

                        if (c.Identity)
                            sql.Append(" generated always as identity primary key");

                        first = false;
                    }
                }
                sql.AppendLine("\n);");
                sql.AppendLine($"COMMENT ON TABLE {FullName} IS '{t.Comment}';");
            }
            else if (DType == SqlObjectType.Schema)
            {
                var t = Definition as string;

                sql.Append($"CREATE SCHEMA IF NOT EXISTS {t};");
            }

            return sql.ToString();
        }
    }
}
