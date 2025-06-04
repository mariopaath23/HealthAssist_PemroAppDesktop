using Microsoft.UI.Xaml;
using HealthAssist.Services;
using HealthAssist.Views;
using Microsoft.UI.Xaml.Controls; 

namespace HealthAssist
{
    public sealed partial class MainWindow : Window
    {
        public Frame AppFrame
        {
            get { return ContentFrame; } 
        }

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "HealthAssist";
            NavigateToInitialPage();
        }

        private async void NavigateToInitialPage()
        {
            var databaseService = new DatabaseService();
            bool isOnboardingCompleted = await databaseService.IsOnboardingCompletedAsync();

            if (isOnboardingCompleted)
            {
                ContentFrame.Navigate(typeof(DashboardPage));
            }
            else
            {
                ContentFrame.Navigate(typeof(OnboardingPage));
            }
        }
    }
}