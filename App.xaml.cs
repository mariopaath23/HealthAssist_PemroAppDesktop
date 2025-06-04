using HealthAssist.Services; // Existing
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls; // Required for Frame
using System.Threading.Tasks;  // Existing

namespace HealthAssist
{
    public partial class App : Application
    {
        public static MainWindow? RootWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            RootWindow = new MainWindow();
            await ApplyAppThemeAsync(RootWindow);
            RootWindow.Activate();
        }

        public static Frame? GetAppFrame()
        {
            if (RootWindow is MainWindow mainWindow)
            {
                return mainWindow.AppFrame; // Use the new public property here
            }
            return null;
        }

        public static async Task ApplyAppThemeAsync(Window window)
        {
            if (window?.Content is FrameworkElement rootElement)
            {
                var dbService = new DatabaseService();
                string? themeSetting = await dbService.GetSettingAsync("AppTheme");

                ElementTheme theme = ElementTheme.Default;
                switch (themeSetting?.ToLowerInvariant())
                {
                    case "light":
                        theme = ElementTheme.Light;
                        break;
                    case "dark":
                        theme = ElementTheme.Dark;
                        break;
                    case "default":
                    case "usesystemsetting":
                    case null:
                        theme = ElementTheme.Default;
                        break;
                }
                rootElement.RequestedTheme = theme;
            }
        }
    }
}