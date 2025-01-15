using Microsoft.Extensions.Configuration;

namespace Sitl.Pdf.Tests {

    public class AppSettings {
        public string WorkingFolder { get; set; } = "C:\\Temp\\Sitl.Pdf.Tests";
        public bool Teardown { get; set; } = true;

        static AppSettings? appSettings;
        public static AppSettings Get() {
            if (appSettings == null) {
                appSettings = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build()
                    .Get<AppSettings>() ?? new AppSettings();

                if (string.IsNullOrEmpty(appSettings.WorkingFolder)) appSettings.WorkingFolder = "C:\\Temp";
            }

            return appSettings;
        }
    }
}
