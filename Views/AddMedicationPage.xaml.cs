using HealthAssist.Models;
using HealthAssist.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthAssist.Views
{
    public sealed partial class AddMedicationPage : Page
    {
        private readonly DatabaseService _databaseService;
        private List<TimePicker> _reminderTimePickers = new List<TimePicker>();

        public AddMedicationPage()
        {
            this.InitializeComponent(); // This initializes RemindersStackPanel and other XAML elements
            _databaseService = new DatabaseService();

            StartDatePicker.SelectedDate = DateTimeOffset.Now;

            // Subscribe to the Loaded event
            this.Loaded += AddMedicationPage_Loaded;
        }

        // Page_Loaded event handler
        private void AddMedicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Now that the page is loaded, XAML elements are guaranteed to be available.
            // Initial call to setup time pickers based on default FrequencyNumberBox value.
            if (FrequencyNumberBox != null) // Should not be null here, but good practice
            {
                UpdateReminderTimePickers((int)FrequencyNumberBox.Value);
            }
            else
            {
                // Fallback or error logging if FrequencyNumberBox is unexpectedly null
                UpdateReminderTimePickers(1); // Default to 1 if something is wrong
            }
        }

        private void FrequencyNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(args.NewValue) && args.NewValue != args.OldValue)
            {
                UpdateReminderTimePickers((int)args.NewValue);
            }
            else if (double.IsNaN(args.NewValue))
            {
                UpdateReminderTimePickers(0);
            }
        }

        private void UpdateReminderTimePickers(int count)
        {
            // This is line 44 (or around it) where the error occurred.
            // RemindersStackPanel should now be instantiated.
            if (RemindersStackPanel == null)
            {
                // This would indicate a deeper issue if it's null after Page.Loaded
                // For now, let's assume it won't be.
                return;
            }

            RemindersStackPanel.Children.Clear();
            _reminderTimePickers.Clear();

            if (count < 0) count = 0;

            for (int i = 0; i < count; i++)
            {
                var timePicker = new TimePicker
                {
                    Header = $"Reminder Time {i + 1}",
                    MinuteIncrement = 1,
                    SelectedTime = new TimeSpan(8 + i * 2, 0, 0)
                };
                RemindersStackPanel.Children.Add(timePicker);
                _reminderTimePickers.Add(timePicker);
            }
        }

        private void SetEndDateCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            EndDatePicker.IsEnabled = SetEndDateCheckBox.IsChecked ?? false;
            if (EndDatePicker.IsEnabled && EndDatePicker.SelectedDate == null)
            {
                EndDatePicker.SelectedDate = StartDatePicker.SelectedDate?.AddDays(7) ?? DateTimeOffset.Now.AddDays(7);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await ValidateFormAsync()) return;

            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            try
            {
                var medication = new Medication
                {
                    Name = MedicationNameTextBox.Text.Trim(),
                    Dosage = DosageTextBox.Text.Trim(),
                    Notes = NotesTextBox.Text.Trim(),
                    StartDate = (StartDatePicker.SelectedDate ?? DateTimeOffset.Now).DateTime,
                    EndDate = (SetEndDateCheckBox.IsChecked == true && EndDatePicker.SelectedDate.HasValue)
                                ? EndDatePicker.SelectedDate.Value.DateTime
                                : (DateTime?)null,
                    IsActive = IsActiveToggleSwitch.IsOn,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                int medicationId = await _databaseService.SaveMedicationAsync(medication);

                if (medicationId > 0 || medication.Id > 0)
                {
                    int actualMedicationId = medicationId > 0 ? medicationId : medication.Id;

                    foreach (var timePicker in _reminderTimePickers)
                    {
                        var reminder = new MedicationReminder
                        {
                            MedicationId = actualMedicationId,
                            ReminderTime = timePicker.SelectedTime ?? new TimeSpan(8, 0, 0),
                            IsActive = true
                        };
                        await _databaseService.SaveMedicationReminderAsync(reminder);
                    }
                }

                await ShowMessageDialogAsync("Success", "Medication saved successfully!");
                if (Frame.CanGoBack) Frame.GoBack();
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to save medication: {ex.Message}");
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
                SaveButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private async Task<bool> ValidateFormAsync()
        {
            if (string.IsNullOrWhiteSpace(MedicationNameTextBox.Text))
            {
                await ShowMessageDialogAsync("Validation Error", "Medication Name is required.");
                MedicationNameTextBox.Focus(FocusState.Programmatic);
                return false;
            }
            if (string.IsNullOrWhiteSpace(DosageTextBox.Text))
            {
                await ShowMessageDialogAsync("Validation Error", "Dosage information is required.");
                DosageTextBox.Focus(FocusState.Programmatic);
                return false;
            }
            if (StartDatePicker.SelectedDate == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Start Date is required.");
                return false;
            }
            if (SetEndDateCheckBox.IsChecked == true && EndDatePicker.SelectedDate == null)
            {
                await ShowMessageDialogAsync("Validation Error", "End Date is required if 'Set End Date' is checked.");
                return false;
            }
            if (SetEndDateCheckBox.IsChecked == true && EndDatePicker.SelectedDate.HasValue && StartDatePicker.SelectedDate.HasValue &&
                EndDatePicker.SelectedDate.Value.Date < StartDatePicker.SelectedDate.Value.Date)
            {
                await ShowMessageDialogAsync("Validation Error", "End Date cannot be before Start Date.");
                return false;
            }
            foreach (var tp in _reminderTimePickers)
            {
                if (tp.SelectedTime == null)
                {
                    await ShowMessageDialogAsync("Validation Error", $"Please select a time for {tp.Header}.");
                    tp.Focus(FocusState.Programmatic);
                    return false;
                }
            }

            return true;
        }

        private async Task ShowMessageDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot // Ensure XamlRoot is set
            };
            await dialog.ShowAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}