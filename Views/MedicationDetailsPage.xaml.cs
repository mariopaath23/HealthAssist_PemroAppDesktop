using HealthAssist.Models;
using HealthAssist.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics; // Required for Debug.WriteLine
using System.Linq;
using System.Threading.Tasks;

namespace HealthAssist.Views
{
    public sealed partial class MedicationDetailsPage : Page
    {
        private readonly DatabaseService _databaseService;
        private int _medicationId;
        private Medication? _currentMedication;
        private List<MedicationReminder> _loadedReminders = new List<MedicationReminder>();
        private List<TimePicker> _reminderEditTimePickers = new List<TimePicker>();

        public MedicationDetailsPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int medId)
            {
                _medicationId = medId;
                await LoadMedicationDetailsAsync();
            }
            else
            {
                if (Frame.CanGoBack) Frame.GoBack();
            }
        }

        private async Task LoadMedicationDetailsAsync()
        {
            LoadingRing.IsActive = true;
            ContentScrollViewer.Visibility = Visibility.Collapsed;

            _currentMedication = await _databaseService.GetMedicationAsync(_medicationId);
            if (_currentMedication == null)
            {
                await ShowMessageDialogAsync("Error", "Medication not found.");
                if (Frame.CanGoBack) Frame.GoBack();
                return;
            }

            _loadedReminders = await _databaseService.GetMedicationRemindersAsync(_medicationId) ?? new List<MedicationReminder>();

            // Populate View Mode
            PageTitleTextBlock.Text = _currentMedication.Name; // Already done, but good to keep track
            NameViewTextBlock.Text = _currentMedication.Name;
            DosageViewTextBlock.Text = _currentMedication.Dosage;
            StartDateViewTextBlock.Text = _currentMedication.StartDate.ToString("MMM dd, yyyy"); // Consistent format

            // Handle End Date display within its new internal StackPanel
            if (_currentMedication.EndDate.HasValue)
            {
                EndDateViewTextBlock.Text = _currentMedication.EndDate.Value.ToString("MMM dd, yyyy");
                EndDateViewStackPanelInternal.Visibility = Visibility.Visible; // Show the internal panel
            }
            else
            {
                EndDateViewTextBlock.Text = "Ongoing"; // Or you can hide the EndDateViewTextBlock too
                EndDateViewStackPanelInternal.Visibility = Visibility.Collapsed; // Hide if no end date
            }

            IsActiveViewTextBlock.Text = _currentMedication.IsActive ? "Active" : "Inactive";

            // Handle Notes Card visibility
            bool hasNotes = !string.IsNullOrWhiteSpace(_currentMedication.Notes);
            if (hasNotes)
            {
                NotesViewTextBlock.Text = _currentMedication.Notes;
                NotesViewCard.Visibility = Visibility.Visible;
            }
            else
            {
                NotesViewTextBlock.Text = ""; // Clear it or set to "No notes." if you prefer the text visible
                NotesViewCard.Visibility = Visibility.Collapsed; // Hide the whole card
            }

            RemindersViewRepeater.ItemsSource = _loadedReminders;
            NoRemindersViewTextBlock.Visibility = _loadedReminders.Any() ? Visibility.Collapsed : Visibility.Visible;

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
            if (_currentMedication == null) return;

            NameEditTextBox.Text = _currentMedication.Name;
            DosageEditTextBox.Text = _currentMedication.Dosage;
            StartDateEditPicker.Date = new DateTimeOffset(_currentMedication.StartDate);

            SetEndDateEditCheckBox.IsChecked = _currentMedication.EndDate.HasValue;
            EndDateEditPicker.IsEnabled = _currentMedication.EndDate.HasValue;

            if (_currentMedication.EndDate.HasValue)
            {
                EndDateEditPicker.Date = new DateTimeOffset(_currentMedication.EndDate.Value);
            }
            else
            {
                // Option 1: assign a placeholder value (e.g., today + 7)
                EndDateEditPicker.Date = DateTimeOffset.Now.AddDays(7);
                // Or just avoid assigning at all if not needed
            }

            IsActiveEditToggleSwitch.IsOn = _currentMedication.IsActive;
            NotesEditTextBox.Text = _currentMedication.Notes;

            FrequencyEditNumberBox.Value = _loadedReminders?.Count ?? 0;
            UpdateReminderTimePickersEditMode((int)FrequencyEditNumberBox.Value, _loadedReminders);

            ViewModePanel.Visibility = Visibility.Collapsed;
            EditModePanel.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            SaveChangesButton.Visibility = Visibility.Visible;
            CancelEditButton.Visibility = Visibility.Visible;
        }

        private void FrequencyEditNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (!double.IsNaN(args.NewValue) && args.NewValue != args.OldValue)
            {
                UpdateReminderTimePickersEditMode((int)args.NewValue, null);
            }
            else if (double.IsNaN(args.NewValue))
            {
                UpdateReminderTimePickersEditMode(0, null);
            }
        }

        private void UpdateReminderTimePickersEditMode(int count, List<MedicationReminder>? existingReminders)
        {
            if (RemindersEditStackPanel == null)
            {
                Debug.WriteLine("ERROR: RemindersEditStackPanel is null.");
                return;
            }

            RemindersEditStackPanel.Children.Clear();
            _reminderEditTimePickers.Clear();

            for (int i = 0; i < count; i++)
            {
                var timePicker = new TimePicker
                {
                    Header = $"Reminder Time {i + 1}",
                    MinuteIncrement = 1
                };

                // ✔️ Check both list and element at index
                if (existingReminders != null && i < existingReminders.Count && existingReminders[i] != null)
                {
                    timePicker.SelectedTime = existingReminders[i].ReminderTime;
                }
                else
                {
                    // Default to something reasonable
                    timePicker.SelectedTime = new TimeSpan(8 + i * 2, 0, 0);
                }

                RemindersEditStackPanel.Children.Add(timePicker);
                _reminderEditTimePickers.Add(timePicker);
            }
        }


        private void SetEndDateEditCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            EndDateEditPicker.IsEnabled = SetEndDateEditCheckBox.IsChecked ?? false;
            DateTimeOffset? startDateFromPicker = StartDateEditPicker.Date;
            if (EndDateEditPicker.IsEnabled && EndDateEditPicker.Date == null && startDateFromPicker.HasValue)
            {
                EndDateEditPicker.Date = startDateFromPicker.Value.AddDays(7);
            }
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
            if (_currentMedication == null || !await ValidateEditFormAsync()) return;

            LoadingRing.IsActive = true;

            _currentMedication.Name = NameEditTextBox.Text.Trim();
            _currentMedication.Dosage = DosageEditTextBox.Text.Trim();

            DateTimeOffset? pickedStartDate = StartDateEditPicker.Date;
            _currentMedication.StartDate = (pickedStartDate.HasValue ? pickedStartDate.Value : new DateTimeOffset(DateTime.Now)).DateTime;

            DateTimeOffset? pickedEndDate = EndDateEditPicker.Date;
            _currentMedication.EndDate = (SetEndDateEditCheckBox.IsChecked == true && pickedEndDate.HasValue)
                                        ? pickedEndDate.Value.DateTime
                                        : (DateTime?)null;
            _currentMedication.IsActive = IsActiveEditToggleSwitch.IsOn;
            _currentMedication.Notes = NotesEditTextBox.Text.Trim();
            _currentMedication.UpdatedAt = DateTime.Now;

            await _databaseService.SaveMedicationAsync(_currentMedication);
            await _databaseService.DeleteMedicationRemindersAsync(_currentMedication.Id);

            foreach (var timePicker in _reminderEditTimePickers)
            {
                if (timePicker.SelectedTime.HasValue)
                {
                    var newReminder = new MedicationReminder
                    {
                        MedicationId = _currentMedication.Id,
                        ReminderTime = timePicker.SelectedTime.Value,
                        IsActive = true
                    };
                    await _databaseService.SaveMedicationReminderAsync(newReminder);
                }
            }
            await LoadMedicationDetailsAsync();
        }

        private async Task<bool> ValidateEditFormAsync()
        {
            if (string.IsNullOrWhiteSpace(NameEditTextBox.Text))
            {
                await ShowMessageDialogAsync("Validation Error", "Medication Name is required.");
                NameEditTextBox.Focus(FocusState.Programmatic);
                return false;
            }

            DateTimeOffset? validateStartDate = StartDateEditPicker.Date;
            if (validateStartDate == null)
            {
                await ShowMessageDialogAsync("Validation Error", "Start Date is required.");
                return false;
            }

            DateTimeOffset? validateEndDate = EndDateEditPicker.Date;
            if (SetEndDateEditCheckBox.IsChecked == true && validateEndDate == null)
            {
                await ShowMessageDialogAsync("Validation Error", "End Date is required if 'Set End Date' is checked.");
                return false;
            }

            if (SetEndDateEditCheckBox.IsChecked == true &&
                validateEndDate.HasValue &&
                validateStartDate.HasValue &&
                validateEndDate.Value.Date < validateStartDate.Value.Date)
            {
                await ShowMessageDialogAsync("Validation Error", "End Date cannot be before Start Date.");
                return false;
            }
            return true;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMedication == null) return;

            var deleteDialog = new ContentDialog
            {
                Title = "Delete Medication",
                Content = $"Are you sure you want to delete '{_currentMedication.Name}'? This will mark it as inactive.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _databaseService.DeleteMedicationAsync(_currentMedication);
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