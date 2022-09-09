using System.Reflection;

namespace Steelax.PgSpell.Helpers
{
    public static class Resource
    {
        public static byte[] Read(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream($"Steelax.PgSpell.{resource}");

            if (stream is null)
                throw new NullReferenceException($"EmbeddedResource {resource} doesn't exist.");

            using var ms = new MemoryStream();

            stream.CopyTo(ms);

            return ms.ToArray();
        }
    }
}
