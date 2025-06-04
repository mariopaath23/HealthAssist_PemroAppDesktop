using HealthAssist.Models;
using HealthAssist.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls; // Required for TeachingTip if used explicitly in C# beyond x:Name
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// using Windows.Globalization.NumberFormatting; // For DecimalFormatter if you use it

namespace HealthAssist.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly DatabaseService _databaseService;
        private User? _currentUser;
        private bool _isThemeComboBoxLoaded = false;

        public SettingsPage()
        {
            this.InitializeComponent(); // This must be the first line
            _databaseService = new DatabaseService();
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateComboBoxes();
            await LoadUserSettingsAsync();
        }

        private void PopulateComboBoxes()
        {
            BloodTypeEditComboBox.ItemsSource = new List<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-", "Unknown" };
            GenderEditComboBox.ItemsSource = new List<string> { "Male", "Female", "Other", "Prefer not to say" };
            ThemeComboBox.ItemsSource = new List<string> { "Use system setting", "Light", "Dark" };
        }

        private async Task LoadUserSettingsAsync()
        {
            LoadingRing.IsActive = true;
            _currentUser = await _databaseService.GetUserAsync();

            if (_currentUser != null)
            {
                NameEditTextBox.Text = _currentUser.Name;
                // DobEditPicker.Date is DateTimeOffset?, _currentUser.DateOfBirth is DateTime
                DobEditPicker.Date = new DateTimeOffset(_currentUser.DateOfBirth);
                BloodTypeEditComboBox.SelectedItem = _currentUser.BloodType;
                AllergiesEditTextBox.Text = _currentUser.Allergies;
                GenderEditComboBox.SelectedItem = _currentUser.Gender;
                HeightEditNumberBox.Value = _currentUser.Height;
                WeightEditNumberBox.Value = _currentUser.Weight;
            }

            string? themeSetting = await _databaseService.GetSettingAsync("AppTheme");
            switch (themeSetting?.ToLowerInvariant())
            {
                case "light":
                    ThemeComboBox.SelectedItem = "Light";
                    break;
                case "dark":
                    ThemeComboBox.SelectedItem = "Dark";
                    break;
                default:
                    ThemeComboBox.SelectedItem = "Use system setting";
                    break;
            }
            _isThemeComboBoxLoaded = true;
            LoadingRing.IsActive = false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        private async void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                await ShowMessageDialogAsync("Error", "User profile not found. Please restart the app.");
                return;
            }

            if (!await ValidateProfileFormAsync()) return;

            LoadingRing.IsActive = true;
            SaveProfileButton.IsEnabled = false;

            try
            {
                _currentUser.Name = NameEditTextBox.Text.Trim();

                // Line 98/99 area in your error list
                DateTimeOffset? pickedDob = DobEditPicker.Date; // Work with nullable local var
                if (pickedDob.HasValue)
                {
                    _currentUser.DateOfBirth = pickedDob.Value.DateTime;
                }
                // else: Validation should ensure it's not null if required.
                // If DOB can be optional (not currently based on validation), handle accordingly.

                _currentUser.BloodType = BloodTypeEditComboBox.SelectedItem as string ?? string.Empty;
                _currentUser.Allergies = AllergiesEditTextBox.Text.Trim();
                _currentUser.Gender = GenderEditComboBox.SelectedItem as string ?? string.Empty;
                _currentUser.Height = HeightEditNumberBox.Value; // Assuming Value is double and not NaN (validation helps)
                _currentUser.Weight = WeightEditNumberBox.Value; // Assuming Value is double and not NaN
                _currentUser.UpdatedAt = DateTime.Now;

                await _databaseService.SaveUserAsync(_currentUser);

                // Lines 108, 109 for SuccessTip
                if (SuccessTip != null) // Check if SuccessTip was initialized
                {
                    SuccessTip.Subtitle = "Profile updated successfully!";
                    SuccessTip.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to save profile: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
                SaveProfileButton.IsEnabled = true;
            }
        }

        private async Task<bool> ValidateProfileFormAsync()
        {
            if (string.IsNullOrWhiteSpace(NameEditTextBox.Text))
            {
                await ShowMessageDialogAsync("Validation Error", "Full Name is required.");
                return false;
            }

            // Line 129 warning area
            DateTimeOffset? validateDob = DobEditPicker.Date; // Use local nullable var
            if (validateDob == null) // This comparison is now fine
            {
                await ShowMessageDialogAsync("Validation Error", "Date of Birth is required.");
                return false;
            }
            if (BloodTypeEditComboBox.SelectedItem == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Blood Type is required.");
                return false;
            }
            if (GenderEditComboBox.SelectedItem == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Gender is required.");
                return false;
            }
            if (double.IsNaN(HeightEditNumberBox.Value) || HeightEditNumberBox.Value <= 0)
            {
                await ShowMessageDialogAsync("Validation Error", "Valid Height is required.");
                return false;
            }
            if (double.IsNaN(WeightEditNumberBox.Value) || WeightEditNumberBox.Value <= 0)
            {
                await ShowMessageDialogAsync("Validation Error", "Valid Weight is required.");
                return false;
            }
            return true;
        }

        private async void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isThemeComboBoxLoaded || ThemeComboBox.SelectedItem == null) return;

            string selectedThemeString = ThemeComboBox.SelectedItem.ToString() ?? "Use system setting";
            ElementTheme themeToApply = ElementTheme.Default;
            string themeToSave = "default";

            switch (selectedThemeString.ToLowerInvariant()) // Use ToLowerInvariant for case-insensitivity
            {
                case "light":
                    themeToApply = ElementTheme.Light;
                    themeToSave = "light";
                    break;
                case "dark":
                    themeToApply = ElementTheme.Dark;
                    themeToSave = "dark";
                    break;
                case "use system setting": // Match XAML item string
                default:
                    themeToApply = ElementTheme.Default;
                    themeToSave = "default"; // Ensure "default" is saved
                    break;
            }

            if (this.XamlRoot?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = themeToApply;
            }
            // Alternatively, call the static method from App.xaml.cs
            // This might require App.xaml.cs to expose its main window or a method to set theme
            // For simplicity, current window root is often sufficient for immediate visual change.
            // If using App.ApplyAppThemeAsync, ensure it can target the current window instance.
            // await App.ApplyAppThemeAsync(Window.Current); // Window.Current might not be available in unpackaged apps

            await _databaseService.SaveSettingAsync("AppTheme", themeToSave);
        }

        private async Task ShowMessageDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}