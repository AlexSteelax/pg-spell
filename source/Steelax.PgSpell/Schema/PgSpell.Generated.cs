//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v10.7.1.0 (Newtonsoft.Json v12.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------


namespace Steelax.PgSpell.Schema
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v12.0.0.0)")]
    public partial class Table
    {
        /// <summary>
        /// The table was created in this specified schema
        /// </summary>
        [Newtonsoft.Json.JsonProperty("schema", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"[_A-z0-9]*$")]
        public string Schema { get; set; } = "public";

        /// <summary>
        /// The name of the table to be created
        /// </summary>
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"[_A-z0-9]*$")]
        public string Name { get; set; }

        /// <summary>
        /// Define or change the comment of an object
        /// </summary>
        [Newtonsoft.Json.JsonProperty("comment", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [Newtonsoft.Json.JsonProperty("options", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Options Options { get; set; }

        [Newtonsoft.Json.JsonProperty("columns", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.ICollection<Column> Columns { get; set; } = new System.Collections.ObjectModel.Collection<Column>();



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v12.0.0.0)")]
    public partial class Composite
    {
        /// <summary>
        /// The type was created in this specified schema
        /// </summary>
        [Newtonsoft.Json.JsonProperty("schema", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"[_A-z0-9]*$")]
        public string Schema { get; set; } = "public";

        /// <summary>
        /// The name of the type to be created
        /// </summary>
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"[_A-z0-9]*$")]
        public string Name { get; set; }

        /// <summary>
        /// Define or change the comment of an object
        /// </summary>
        [Newtonsoft.Json.JsonProperty("comment", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Comment { get; set; }

        [Newtonsoft.Json.JsonProperty("columns", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<Column> Columns { get; set; }



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v12.0.0.0)")]
    public partial class Column
    {
        /// <summary>
        /// The data type of the column. This can include array specifiers.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Type { get; set; }

        /// <summary>
        /// The name of a column to be created in the new table.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"[_A-z0-9]*$")]
        public string Name { get; set; }

        /// <summary>
        /// Column can be null or not.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("nullable", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool Nullable { get; set; } = true;

        /// <summary>
        /// Define or change the comment of an object
        /// </summary>
        [Newtonsoft.Json.JsonProperty("comment", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Comment { get; set; }



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v12.0.0.0)")]
    public partial class PgSpellCollection
    {
        /// <summary>
        /// Table declarations
        /// </summary>
        [Newtonsoft.Json.JsonProperty("tables", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<Table> Tables { get; set; }

        /// <summary>
        /// Composite type declarations
        /// </summary>
        [Newtonsoft.Json.JsonProperty("composites", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<Composite> Composites { get; set; }

        /// <summary>
        /// Enum type declarations
        /// </summary>
        [Newtonsoft.Json.JsonProperty("enums", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.ICollection<string> Enums { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.7.1.0 (Newtonsoft.Json v12.0.0.0)")]
    public partial class Options
    {
        /// <summary>
        /// If specified, the table is created as an unlogged table. Data written to unlogged tables is not written to the write-ahead log, which makes them considerably faster than ordinary tables. However, they are not crash-safe: an unlogged table is automatically truncated after a crash or unclean shutdown. The contents of an unlogged table are also not replicated to standby servers. Any indexes created on an unlogged table are automatically unlogged as well.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("unlogged", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool Unlogged { get; set; } = false;



        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }

    }
}