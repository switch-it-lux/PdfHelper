using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace Sit.Pdf.Tests {

    public abstract class TestBase(ITestOutputHelper output) {
        protected ITestOutputHelper Output => output;
        protected AppSettings AppSettings => AppSettings.Get();

        protected string GetTestFileName(string suffix = "", string extension = ".pdf", [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "") {
            if (!string.IsNullOrEmpty(extension) && extension[0] != '.') extension = $".{extension}";
            string callerClassName = !string.IsNullOrEmpty(callerFilePath) ? $"{Path.GetFileNameWithoutExtension(callerFilePath)}." : "";

            return Path.Combine(AppSettings.WorkingFolder, $"{callerClassName}{callerMemberName}{suffix}{extension}");
        }
    }
}