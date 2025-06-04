using HealthAssist.Services; // For DatabaseService

using Microsoft.UI.Xaml;

using System.Threading.Tasks; // For Task


namespace HealthAssist

{

    public partial class App : Application

    {

        public App()

        {

            this.InitializeComponent();

        }


        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)

        {

            m_window = new MainWindow();


            // Apply theme before activating the window

            await ApplyAppThemeAsync(m_window);


            m_window.Activate();

        }


        private Window? m_window;


        // Method to apply the stored theme

        public static async Task ApplyAppThemeAsync(Window window)

        {

            if (window?.Content is FrameworkElement rootElement)

            {

                var dbService = new DatabaseService(); // Consider DI or a singleton for service access

                string? themeSetting = await dbService.GetSettingAsync("AppTheme");


                ElementTheme theme = ElementTheme.Default; // Default to system

                switch (themeSetting?.ToLowerInvariant())

                {

                    case "light":

                        theme = ElementTheme.Light;

                        break;

                    case "dark":

                        theme = ElementTheme.Dark;

                        break;

                    case "default": // Explicitly handle "default" or "use system setting"

                    case "usesystemsetting": // In case you save it this way

                    case null: // If setting is not found, use system default

                        theme = ElementTheme.Default;

                        break;

                }

                rootElement.RequestedTheme = theme;

            }

        }

    }

}