using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using HealthAssist.Models;
using HealthAssist.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthAssist.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly DatabaseService _databaseService;
        private User? _currentUser;

        public DashboardPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                _currentUser = await _databaseService.GetUserAsync();
                if (_currentUser != null)
                {
                    WelcomeTextBlock.Text = $"Welcome back, {_currentUser.Name}!";
                }

                DateTextBlock.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");

                // Load medications
                var medications = await _databaseService.GetMedicationsAsync();
                MedicationCountTextBlock.Text = medications.Count.ToString();
                MedicationsListView.ItemsSource = medications.Take(3).ToList();

                // Load appointments
                var appointments = await _databaseService.GetAppointmentsAsync();
                AppointmentCountTextBlock.Text = appointments.Count.ToString();
                AppointmentsListView.ItemsSource = appointments.Take(3).ToList();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to load dashboard data: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void AddMedicationButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddMedicationPage));
        }

        private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddAppointmentPage));
        }

        private void ViewAllMedicationsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MedicationsListPage));
        }

        private void ViewAllAppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AppointmentsListPage));
        }

        private void MedicationsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Medication medication)
            {
                Frame.Navigate(typeof(MedicationDetailsPage), medication.Id);
            }
        }

        private void AppointmentsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Appointment appointment)
            {
                Frame.Navigate(typeof(AppointmentDetailsPage), appointment.Id);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}