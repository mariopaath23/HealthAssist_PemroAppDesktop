using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HealthAssist.Models;
using HealthAssist.Services;
using System;


namespace HealthAssist.Views
{
    public sealed partial class OnboardingPage : Page
    {
        private readonly DatabaseService _databaseService;

        public OnboardingPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private async void CompleteOnboardingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            CompleteOnboardingButton.IsEnabled = false;

            try
            {
                var user = new User
                {
                    Name = NameTextBox.Text.Trim(),
                    DateOfBirth = DateOfBirthPicker.Date.DateTime,
                    BloodType = ((ComboBoxItem)BloodTypeComboBox.SelectedItem)?.Content?.ToString() ?? "",
                    Allergies = AllergiesTextBox.Text.Trim(),
                    Gender = ((ComboBoxItem)GenderComboBox.SelectedItem)?.Content?.ToString() ?? "",
                    Height = HeightNumberBox.Value,
                    Weight = WeightNumberBox.Value
                };

                await _databaseService.SaveUserAsync(user);

                // Navigate to dashboard
                Frame.Navigate(typeof(DashboardPage));
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to save user data: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                CompleteOnboardingButton.IsEnabled = true;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowValidationError("Please enter your name.");
                NameTextBox.Focus(FocusState.Programmatic);
                return false;
            }

            if (DateOfBirthPicker.Date == null)
            {
                ShowValidationError("Please select your date of birth.");
                return false;
            }

            if (BloodTypeComboBox.SelectedItem == null)
            {
                ShowValidationError("Please select your blood type.");
                return false;
            }

            if (GenderComboBox.SelectedItem == null)
            {
                ShowValidationError("Please select your gender.");
                return false;
            }

            if (HeightNumberBox.Value <= 0)
            {
                ShowValidationError("Please enter a valid height.");
                return false;
            }

            if (WeightNumberBox.Value <= 0)
            {
                ShowValidationError("Please enter a valid weight.");
                return false;
            }

            return true;
        }

        private async void ShowValidationError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Validation Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
