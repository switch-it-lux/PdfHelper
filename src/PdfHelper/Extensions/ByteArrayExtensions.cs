using System;
using System.IO;

namespace Sit.Pdf {

    internal static class ByteArrayExtensions {

        public static double GetSizeInBytes(this byte[] bytes) {
            return bytes?.Length ?? 0.0;
        }

        public static double EvaluateSizeOnDiskInBytes(this byte[] bytes, string tempFolderPath = null) {
            if (string.IsNullOrEmpty(tempFolderPath)) tempFolderPath = Path.GetTempPath();
            Directory.CreateDirectory(tempFolderPath);

            string path = Path.Combine(tempFolderPath, new Guid().ToString() + ".pdf");
            File.WriteAllBytes(path, bytes);
            var size = new FileInfo(path).Length;
            File.Delete(path);
            return size;
        }

        public static double GetSizeInKb(this byte[] bytes) {
            return (bytes?.Length ?? 0) / 1024.0;
        }

        public static double EvaluateSizeOnDiskInKb(this byte[] bytes, string tempFolderPath = null) {
            return bytes.EvaluateSizeOnDiskInBytes(tempFolderPath) / 1024.0;
        }

        public static double GetSizeInMb(this byte[] bytes) {
            return (bytes?.Length ?? 0) / (1024.0 * 1024.0);
        }

        public static double EvaluateSizeOnDiskInMb(this byte[] bytes, string tempFolderPath = null) {
            return bytes.EvaluateSizeOnDiskInBytes(tempFolderPath) / (1024.0 * 1024.0);
        }
    }
}
