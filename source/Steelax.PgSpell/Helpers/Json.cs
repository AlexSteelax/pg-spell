using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;

namespace Steelax.PgSpell.Helpers
{
    public static class Json
    {
        public static ICollection<ValidationError> Validate<T>(this JsonSchema schema, T data)
        {
            var jtoken = JToken.FromObject(data, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return schema.Validate(jtoken);
        }
    }
}
