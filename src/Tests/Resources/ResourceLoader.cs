using System.Reflection;

namespace Sitl.Pdf.Tests.Resources {

    internal static class ResourceLoader {

        public static string ReadAsString(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"{typeof(ResourceLoader).Namespace}.{resourceName}") ?? throw new ArgumentException($"Embedded resource '{resourceName}' not found");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static byte[] ReadAsBytes(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"{typeof(ResourceLoader).Namespace}.{resourceName}") ?? throw new ArgumentException($"Embedded resource '{resourceName}' not found");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public static Stream ReadAsStream(string resourceName) {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"{typeof(ResourceLoader).Namespace}.{resourceName}") ?? throw new ArgumentException($"Embedded resource '{resourceName}' not found");
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms;
        }
    }
}
