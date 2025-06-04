using HealthAssist.Models;
using HealthAssist.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthAssist.Views
{
    public sealed partial class AppointmentDetailsPage : Page
    {
        private readonly DatabaseService _databaseService;
        private int _appointmentId;
        private Appointment? _currentAppointment;
        private AppointmentReminder? _currentReminder;

        public AppointmentDetailsPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
            this.Loaded += AppointmentDetailsPage_Loaded;
        }

        private void AppointmentDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ReminderFrequencyEditComboBox != null)
            {
                ReminderFrequencyEditComboBox.ItemsSource = Enum.GetValues(typeof(ReminderFrequency));
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int apptId)
            {
                _appointmentId = apptId;
                await LoadAppointmentDetailsAsync();
            }
            else
            {
                if (Frame.CanGoBack) Frame.GoBack();
            }
        }

        private async Task LoadAppointmentDetailsAsync()
        {
            if (LoadingRing == null) return; // Safety check if InitializeComponent failed

            LoadingRing.IsActive = true;
            ContentScrollViewer.Visibility = Visibility.Collapsed;

            _currentAppointment = await _databaseService.GetAppointmentAsync(_appointmentId);
            if (_currentAppointment == null)
            {
                await ShowMessageDialogAsync("Error", "Appointment not found.");
                if (Frame.CanGoBack) Frame.GoBack();
                return;
            }

            var reminders = await _databaseService.GetAppointmentRemindersAsync(_appointmentId);
            _currentReminder = reminders?.FirstOrDefault(r => r.IsActive);

            PageTitleTextBlock.Text = _currentAppointment.Title;
            TitleViewTextBlock.Text = _currentAppointment.Title;

            bool hasDoctor = !string.IsNullOrWhiteSpace(_currentAppointment.DoctorName);
            DoctorNameViewTextBlock.Text = hasDoctor ? _currentAppointment.DoctorName : "N/A";
            DoctorViewCard.Visibility = hasDoctor ? Visibility.Visible : Visibility.Collapsed; // Changed from DoctorViewStackPanel

            DateTimeViewTextBlock.Text = _currentAppointment.AppointmentDateTime.ToString("dddd, MMM dd, yyyy 'at' h:mm tt");
            StatusViewTextBlock.Text = _currentAppointment.IsCompleted ? "Completed" : "Upcoming";

            bool hasNotes = !string.IsNullOrWhiteSpace(_currentAppointment.Notes);
            NotesViewTextBlock.Text = hasNotes ? _currentAppointment.Notes : "No notes.";
            NotesViewCard.Visibility = hasNotes ? Visibility.Visible : Visibility.Collapsed; // Changed from NotesViewStackPanel

            if (_currentReminder != null)
            {
                ReminderViewPanel.Visibility = Visibility.Visible;
                NoReminderViewTextBlock.Visibility = Visibility.Collapsed;
                ReminderFrequencyViewTextBlock.Text = $"{_currentReminder.Frequency}";
                ReminderDaysBeforeViewTextBlock.Text = $"{_currentReminder.DaysBefore}";
                ReminderTimeViewTextBlock.Text = $"{_currentReminder.ReminderTime:hh\\:mm}";
            }
            else
            {
                ReminderViewPanel.Visibility = Visibility.Collapsed;
                NoReminderViewTextBlock.Visibility = Visibility.Visible;
            }

            SwitchToViewMode();
            LoadingRing.IsActive = false;
            ContentScrollViewer.Visibility = Visibility.Visible;
        }

        private void SwitchToViewMode()
        {
            ViewModePanel.Visibility = Visibility.Visible;
            EditModePanel.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            SaveChangesButton.Visibility = Visibility.Collapsed;
            CancelEditButton.Visibility = Visibility.Collapsed;
        }

        private void SwitchToEditMode()
        {
            if (_currentAppointment == null) return;

            TitleEditTextBox.Text = _currentAppointment.Title;
            DoctorNameEditTextBox.Text = _currentAppointment.DoctorName;
            AppointmentDateEditPicker.Date = new DateTimeOffset(_currentAppointment.AppointmentDateTime.Date);
            AppointmentTimeEditPicker.Time = _currentAppointment.AppointmentDateTime.TimeOfDay;
            IsCompletedEditToggleSwitch.IsOn = _currentAppointment.IsCompleted;
            NotesEditTextBox.Text = _currentAppointment.Notes;

            EnableReminderEditCheckBox.IsChecked = _currentReminder != null && _currentReminder.IsActive;
            ReminderEditControlsPanel.Visibility = EnableReminderEditCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (_currentReminder != null && _currentReminder.IsActive)
            {
                ReminderFrequencyEditComboBox.SelectedItem = _currentReminder.Frequency;
                DaysBeforeEditNumberBox.Value = _currentReminder.DaysBefore;
                ReminderTimeEditPicker.SelectedTime = _currentReminder.ReminderTime;
            }
            else
            {
                ReminderFrequencyEditComboBox.SelectedItem = ReminderFrequency.Daily;
                DaysBeforeEditNumberBox.Value = 1;
                ReminderTimeEditPicker.SelectedTime = new TimeSpan(9, 0, 0);
            }

            ViewModePanel.Visibility = Visibility.Collapsed;
            EditModePanel.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            SaveChangesButton.Visibility = Visibility.Visible;
            CancelEditButton.Visibility = Visibility.Visible;
        }

        private void EnableReminderEditCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ReminderEditControlsPanel.Visibility = EnableReminderEditCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToEditMode();
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToViewMode();
        }

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAppointment == null || !await ValidateEditFormAsync()) return;

            LoadingRing.IsActive = true;

            _currentAppointment.Title = TitleEditTextBox.Text.Trim();
            _currentAppointment.DoctorName = DoctorNameEditTextBox.Text.Trim();

            DateTimeOffset? selectedDate = AppointmentDateEditPicker.Date;
            TimeSpan selectedTime = AppointmentTimeEditPicker.SelectedTime ?? new TimeSpan(0, 0, 0);
            if (selectedDate.HasValue)
            {
                _currentAppointment.AppointmentDateTime = new DateTime(selectedDate.Value.Year, selectedDate.Value.Month, selectedDate.Value.Day,
                                                                selectedTime.Hours, selectedTime.Minutes, selectedTime.Seconds);
            }

            _currentAppointment.IsCompleted = IsCompletedEditToggleSwitch.IsOn;
            _currentAppointment.Notes = NotesEditTextBox.Text.Trim();
            _currentAppointment.UpdatedAt = DateTime.Now;

            await _databaseService.SaveAppointmentAsync(_currentAppointment);

            if (EnableReminderEditCheckBox.IsChecked == true)
            {
                if (_currentReminder == null)
                {
                    _currentReminder = new AppointmentReminder { AppointmentId = _currentAppointment.Id };
                }
                _currentReminder.Frequency = (ReminderFrequency)(ReminderFrequencyEditComboBox.SelectedItem ?? ReminderFrequency.Daily);
                _currentReminder.DaysBefore = (int)DaysBeforeEditNumberBox.Value;
                _currentReminder.ReminderTime = ReminderTimeEditPicker.SelectedTime ?? new TimeSpan(9, 0, 0);
                _currentReminder.IsActive = true;
                await _databaseService.SaveAppointmentReminderAsync(_currentReminder);
            }
            else
            {
                if (_currentReminder != null && _currentReminder.Id != 0)
                {
                    _currentReminder.IsActive = false;
                    await _databaseService.SaveAppointmentReminderAsync(_currentReminder);
                }
            }

            await LoadAppointmentDetailsAsync();
        }

        // This method was missing or had issues previously
        private async Task<bool> ValidateEditFormAsync()
        {
            if (string.IsNullOrWhiteSpace(TitleEditTextBox.Text))
            {
                await ShowMessageDialogAsync("Validation Error", "Appointment Title is required.");
                TitleEditTextBox.Focus(FocusState.Programmatic);
                return false;
            }
            if (AppointmentDateEditPicker.Date == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Appointment Date is required.");
                AppointmentDateEditPicker.Focus(FocusState.Programmatic);
                return false;
            }
            // Add more validation for reminder fields if EnableReminderEditCheckBox.IsChecked == true
            if (EnableReminderEditCheckBox.IsChecked == true)
            {
                if (ReminderFrequencyEditComboBox.SelectedItem == null)
                {
                    await ShowMessageDialogAsync("Validation Error", "Reminder Frequency is required.");
                    return false;
                }
                if (double.IsNaN(DaysBeforeEditNumberBox.Value) || DaysBeforeEditNumberBox.Value < 0)
                {
                    await ShowMessageDialogAsync("Validation Error", "'Days before' must be zero or a positive number.");
                    return false;
                }
                if (ReminderTimeEditPicker.SelectedTime == null)
                {
                    await ShowMessageDialogAsync("Validation Error", "Reminder Time is required.");
                    return false;
                }
            }
            return true;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAppointment == null) return;

            var deleteDialog = new ContentDialog
            {
                Title = "Delete Appointment",
                Content = $"Are you sure you want to delete the appointment '{_currentAppointment.Title}'? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (_currentReminder != null && _currentReminder.Id != 0)
                {
                    // To keep it simple, make the reminder inactive when deleting the appointment.
                    // Or you can implement a hard delete for the reminder if preferred.
                    _currentReminder.IsActive = false;
                    await _databaseService.SaveAppointmentReminderAsync(_currentReminder);
                }
                await _databaseService.DeleteAppointmentAsync(_currentAppointment);
                if (Frame.CanGoBack) Frame.GoBack();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        // This method was missing or had issues previously
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
    } // Make sure this is the final closing brace for the class
} // Final closing brace for the namespace