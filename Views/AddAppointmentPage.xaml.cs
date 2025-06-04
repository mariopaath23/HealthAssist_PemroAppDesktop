using HealthAssist.Models;
using HealthAssist.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace HealthAssist.Views
{
    public sealed partial class AddAppointmentPage : Page
    {
        private readonly DatabaseService _databaseService;

        public AddAppointmentPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
            this.Loaded += AddAppointmentPage_Loaded;
        }

        private void AddAppointmentPage_Loaded(object sender, RoutedEventArgs e)
        {
            AppointmentDatePicker.Date = DateTimeOffset.Now; // Default to today
            // Populate ReminderFrequency ComboBox
            ReminderFrequencyComboBox.ItemsSource = Enum.GetValues(typeof(ReminderFrequency));
            ReminderFrequencyComboBox.SelectedItem = ReminderFrequency.Daily; // Default selection
        }

        private void SetReminderCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ReminderSettingsPanel.Visibility = (SetReminderCheckBox.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
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
                DateTimeOffset? selectedDate = AppointmentDatePicker.Date;
                TimeSpan selectedTime = AppointmentTimePicker.SelectedTime ?? new TimeSpan(9, 0, 0);

                if (selectedDate == null)
                {
                    await ShowMessageDialogAsync("Validation Error", "Appointment Date is required.");
                    AppointmentDatePicker.Focus(FocusState.Programmatic);
                    return; // Should be caught by ValidateFormAsync, but good to double check
                }

                DateTime appointmentDateTime = new DateTime(selectedDate.Value.Year, selectedDate.Value.Month, selectedDate.Value.Day,
                                                            selectedTime.Hours, selectedTime.Minutes, selectedTime.Seconds);

                var appointment = new Appointment
                {
                    Title = TitleTextBox.Text.Trim(),
                    DoctorName = DoctorNameTextBox.Text.Trim(),
                    AppointmentDateTime = appointmentDateTime,
                    Notes = NotesTextBox.Text.Trim(),
                    IsCompleted = false, // New appointments are not completed
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                int appointmentId = await _databaseService.SaveAppointmentAsync(appointment);

                if (appointmentId > 0 && SetReminderCheckBox.IsChecked == true)
                {
                    var reminder = new AppointmentReminder
                    {
                        AppointmentId = appointmentId,
                        Frequency = (ReminderFrequency)(ReminderFrequencyComboBox.SelectedItem ?? ReminderFrequency.Daily),
                        DaysBefore = (int)(DaysBeforeNumberBox.Value), // Value is double
                        ReminderTime = ReminderTimePicker.SelectedTime ?? new TimeSpan(8, 0, 0),
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    await _databaseService.SaveAppointmentReminderAsync(reminder);
                }

                await ShowMessageDialogAsync("Success", "Appointment saved successfully!");
                if (Frame.CanGoBack) Frame.GoBack();
            }
            catch (Exception ex)
            {
                await ShowMessageDialogAsync("Error", $"Failed to save appointment: {ex.Message}");
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
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                await ShowMessageDialogAsync("Validation Error", "Appointment Title is required.");
                TitleTextBox.Focus(FocusState.Programmatic);
                return false;
            }

            if (AppointmentDatePicker.Date == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Appointment Date is required.");
                AppointmentDatePicker.Focus(FocusState.Programmatic);
                return false;
            }

            // If combined date is in the past (optional check)
            DateTimeOffset? selectedDate = AppointmentDatePicker.Date;
            TimeSpan selectedTime = AppointmentTimePicker.SelectedTime ?? new TimeSpan(0, 0, 0);
            if (selectedDate.HasValue)
            {
                DateTime appointmentDateTime = new DateTime(selectedDate.Value.Year, selectedDate.Value.Month, selectedDate.Value.Day,
                                                           selectedTime.Hours, selectedTime.Minutes, selectedTime.Seconds);
                if (appointmentDateTime < DateTime.Now.AddMinutes(-1)) // Allow for slight delay in saving
                {
                    //await ShowMessageDialogAsync("Validation Error", "Appointment date and time cannot be in the past.");
                    //return false; 
                    // Decided to comment this out for now, user might want to log past appointments. Can be re-enabled.
                }
            }


            if (SetReminderCheckBox.IsChecked == true)
            {
                if (ReminderFrequencyComboBox.SelectedItem == null)
                {
                    await ShowMessageDialogAsync("Validation Error", "Reminder Frequency is required when setting a reminder.");
                    ReminderFrequencyComboBox.Focus(FocusState.Programmatic);
                    return false;
                }
                if (double.IsNaN(DaysBeforeNumberBox.Value) || DaysBeforeNumberBox.Value < 0)
                {
                    await ShowMessageDialogAsync("Validation Error", "'Days before' must be zero or a positive number.");
                    DaysBeforeNumberBox.Focus(FocusState.Programmatic);
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
                XamlRoot = this.XamlRoot
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