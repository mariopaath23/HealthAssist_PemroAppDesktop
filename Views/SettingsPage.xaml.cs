using HealthAssist.Models;
using HealthAssist.Services;
using HealthAssist.Views; // Required for OnboardingPage
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace HealthAssist.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly DatabaseService _databaseService;
        private User? _currentUser;
        private bool _isThemeComboBoxLoaded = false;

        public SettingsPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateComboBoxes(); //
            await LoadUserSettingsAsync(); //
        }

        private void PopulateComboBoxes()
        {
            BloodTypeEditComboBox.ItemsSource = new List<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Unknown" }; //
            GenderEditComboBox.ItemsSource = new List<string> { "Male", "Female", "Other", "Prefer not to say" }; //
            ThemeComboBox.ItemsSource = new List<string> { "Use system setting", "Light", "Dark" }; //
        }

        private async Task LoadUserSettingsAsync()
        {
            LoadingRing.IsActive = true; //
            _currentUser = await _databaseService.GetUserAsync(); //

            if (_currentUser != null)
            {
                NameEditTextBox.Text = _currentUser.Name; //
                DobEditPicker.Date = new DateTimeOffset(_currentUser.DateOfBirth); //
                BloodTypeEditComboBox.SelectedItem = _currentUser.BloodType; //
                AllergiesEditTextBox.Text = _currentUser.Allergies; //
                GenderEditComboBox.SelectedItem = _currentUser.Gender; //
                HeightEditNumberBox.Value = _currentUser.Height; //
                WeightEditNumberBox.Value = _currentUser.Weight; //
            }

            string? themeSetting = await _databaseService.GetSettingAsync("AppTheme"); //
            switch (themeSetting?.ToLowerInvariant())
            {
                case "light":
                    ThemeComboBox.SelectedItem = "Light"; //
                    break;
                case "dark":
                    ThemeComboBox.SelectedItem = "Dark"; //
                    break;
                default:
                    ThemeComboBox.SelectedItem = "Use system setting"; //
                    break;
            }
            _isThemeComboBoxLoaded = true; //
            LoadingRing.IsActive = false; //
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack(); //
        }

        private async void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessageDialogAsync("Error", "User profile not found. Please restart the app."); //
                return;
            }

            if (!await ValidateProfileFormAsync()) return; //

            LoadingRing.IsActive = true; //
            SaveProfileButton.IsEnabled = false; //

            try
            {
                _currentUser.Name = NameEditTextBox.Text.Trim(); //

                DateTimeOffset? pickedDob = DobEditPicker.Date; //
                if (pickedDob.HasValue)
                {
                    _currentUser.DateOfBirth = pickedDob.Value.DateTime; //
                }

                _currentUser.BloodType = BloodTypeEditComboBox.SelectedItem as string ?? string.Empty; //
                _currentUser.Allergies = AllergiesEditTextBox.Text.Trim(); //
                _currentUser.Gender = GenderEditComboBox.SelectedItem as string ?? string.Empty; //
                _currentUser.Height = HeightEditNumberBox.Value; //
                _currentUser.Weight = WeightEditNumberBox.Value; //
                _currentUser.UpdatedAt = DateTime.Now; //

                await _databaseService.SaveUserAsync(_currentUser); //

                if (SuccessTip != null)
                {
                    SuccessTip.Subtitle = "Profile updated successfully!"; //
                    SuccessTip.IsOpen = true; //
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to save profile: {ex.Message}"); //
            }
            finally
            {
                LoadingRing.IsActive = false; //
                SaveProfileButton.IsEnabled = true; //
            }
        }

        private async Task<bool> ValidateProfileFormAsync()
        {
            if (string.IsNullOrWhiteSpace(NameEditTextBox.Text)) //
            {
                await ShowMessageDialogAsync("Validation Error", "Full Name is required."); //
                return false;
            }

            DateTimeOffset? validateDob = DobEditPicker.Date; //
            if (validateDob == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Date of Birth is required."); //
                return false;
            }
            if (BloodTypeEditComboBox.SelectedItem == null) //
            {
                await ShowMessageDialogAsync("Validation Error", "Blood Type is required."); //
                return false;
            }
            if (GenderEditComboBox.SelectedItem == null) //
            {
                await ShowMessageDialogAsync("Validation Error", "Gender is required."); //
                return false;
            }
            if (double.IsNaN(HeightEditNumberBox.Value) || HeightEditNumberBox.Value <= 0) //
            {
                await ShowMessageDialogAsync("Validation Error", "Valid Height is required."); //
                return false;
            }
            if (double.IsNaN(WeightEditNumberBox.Value) || WeightEditNumberBox.Value <= 0) //
            {
                await ShowMessageDialogAsync("Validation Error", "Valid Weight is required."); //
                return false;
            }
            return true;
        }

        private async void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isThemeComboBoxLoaded || ThemeComboBox.SelectedItem == null) return; //

            string selectedThemeString = ThemeComboBox.SelectedItem.ToString() ?? "Use system setting"; //
            ElementTheme themeToApply = ElementTheme.Default; //
            string themeToSave = "default"; //

            switch (selectedThemeString.ToLowerInvariant())
            {
                case "light":
                    themeToApply = ElementTheme.Light; //
                    themeToSave = "light"; //
                    break;
                case "dark":
                    themeToApply = ElementTheme.Dark; //
                    themeToSave = "dark"; //
                    break;
                case "use system setting":
                default:
                    themeToApply = ElementTheme.Default; //
                    themeToSave = "default"; //
                    break;
            }

            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = themeToApply; //
            }

            await _databaseService.SaveSettingAsync("AppTheme", themeToSave); //
        }

        private async Task ShowMessageDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title, //
                Content = message, //
                CloseButtonText = "OK", //
                XamlRoot = this.XamlRoot //
            };
            await dialog.ShowAsync(); //
        }

        // New event handler for the Reset Application Button
        // New event handler for the Reset Application Button
        private async void ResetApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmationDialog = new ContentDialog
            {
                Title = "Confirm Reset",
                Content = "Are you absolutely sure you want to delete all data and reset the application? This action cannot be undone.",
                PrimaryButtonText = "Yes, Reset Everything",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmationDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                LoadingRing.IsActive = true;
                ResetApplicationButton.IsEnabled = false;
                SaveProfileButton.IsEnabled = false;

                try
                {
                    await _databaseService.DeleteDatabaseAsync();

                    // **MODIFIED NAVIGATION LOGIC HERE**
                    var appNavigationFrame = App.GetAppFrame(); // Use the helper from App.xaml.cs
                    if (appNavigationFrame != null)
                    {
                        appNavigationFrame.Navigate(typeof(OnboardingPage));
                    }
                    else
                    {
                        // Fallback or critical error: This indicates a problem with accessing the main frame.
                        // For safety, you could try the page's own frame, but it's less ideal.
                        System.Diagnostics.Debug.WriteLine("CRITICAL: Could not get main app frame for navigation from App.GetAppFrame(). Attempting fallback.");
                        this.Frame.Navigate(typeof(OnboardingPage));
                        // If this also fails, there's a more fundamental navigation setup issue.
                    }
                }
                catch (Exception ex)
                {
                    await ShowMessageDialogAsync("Error", $"Failed to reset application: {ex.Message}");
                    ResetApplicationButton.IsEnabled = true;
                    SaveProfileButton.IsEnabled = true;
                }
                finally
                {
                    LoadingRing.IsActive = false;
                }
            }
        }
    }
}