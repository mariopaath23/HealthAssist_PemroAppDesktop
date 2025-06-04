using HealthAssist.Models;
using HealthAssist.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HealthAssist.Views
{
    public sealed partial class AppointmentsListPage : Page
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<Appointment> Appointments { get; set; }

        public AppointmentsListPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
            Appointments = new ObservableCollection<Appointment>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadAppointmentsAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async Task LoadAppointmentsAsync()
        {
            LoadingRing.IsActive = true;
            AppointmentsListView.Visibility = Visibility.Collapsed;
            EmptyMessageTextBlock.Visibility = Visibility.Collapsed;

            try
            {
                var appointmentsData = await _databaseService.GetAppointmentsAsync(); // This already gets upcoming and sorts them
                Appointments.Clear();
                if (appointmentsData != null)
                {
                    foreach (var appt in appointmentsData) // Already sorted by date by the service
                    {
                        Appointments.Add(appt);
                    }
                }

                AppointmentsListView.ItemsSource = Appointments;

                if (Appointments.Any())
                {
                    AppointmentsListView.Visibility = Visibility.Visible;
                }
                else
                {
                    EmptyMessageTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                EmptyMessageTextBlock.Text = $"Error loading appointments: {ex.Message}";
                EmptyMessageTextBlock.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
        }

        private void AppointmentsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Appointment selectedAppointment)
            {
                // Navigate to AppointmentDetailsPage (You'll create this page next)
                // Frame.Navigate(typeof(AppointmentDetailsPage), selectedAppointment.Id);
                ShowMessageDialogAsync("Navigate", $"TODO: Navigate to details for {selectedAppointment.Title} (ID: {selectedAppointment.Id})");
            }
        }

        private void AddAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddAppointmentPage));
        }

        private async void ShowMessageDialogAsync(string title, string message)
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