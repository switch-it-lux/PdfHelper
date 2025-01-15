using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("Sit.Pdf.Tests.AssemblyInitialize", "Sit.PdfHelper.Tests")]

namespace Sit.Pdf.Tests {

    public sealed class AssemblyInitialize : XunitTestFramework, IDisposable {
        readonly AppSettings appSettings;

        public AssemblyInitialize(IMessageSink messageSink) : base(messageSink) {
            appSettings = AppSettings.Get();

            if (Directory.Exists(appSettings.WorkingFolder)) 
                Directory.Delete(appSettings.WorkingFolder, true);
            Directory.CreateDirectory(appSettings.WorkingFolder);
        }

        public new void Dispose() {
            if (appSettings.Teardown) {
                if (Directory.Exists(appSettings.WorkingFolder)) 
                    Directory.Delete(appSettings.WorkingFolder, true);
            }

            base.Dispose();
        }
    }
}