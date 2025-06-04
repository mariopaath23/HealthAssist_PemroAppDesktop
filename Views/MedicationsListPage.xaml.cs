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
    public sealed partial class MedicationsListPage : Page
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<Medication> Medications { get; set; }

        public MedicationsListPage()
        {
            this.InitializeComponent();
            _databaseService = new DatabaseService();
            Medications = new ObservableCollection<Medication>();
            // MedicationsListView.ItemsSource = Medications; // Can be set here if Medications prop uses INPC or if x:Bind is used
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadMedicationsAsync(); // Awaiting the Task
        }

        // Corrected method signature and usage
        private async Task LoadMedicationsAsync() // << --- This signature is correct
        {
            LoadingRing.IsActive = true;
            MedicationsListView.Visibility = Visibility.Collapsed;
            EmptyMessageTextBlock.Visibility = Visibility.Collapsed;

            try
            {
                var medicationsData = await _databaseService.GetMedicationsAsync();
                Medications.Clear();
                if (medicationsData != null)
                {
                    foreach (var med in medicationsData.OrderByDescending(m => m.CreatedAt))
                    {
                        Medications.Add(med);
                    }
                }

                MedicationsListView.ItemsSource = Medications; // Set ItemsSource after populating

                if (Medications.Any())
                {
                    MedicationsListView.Visibility = Visibility.Visible;
                }
                else
                {
                    EmptyMessageTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex) // << --- 'ex' is now used
            {
                EmptyMessageTextBlock.Text = $"Error loading medications: {ex.Message}"; // Using ex.Message
                EmptyMessageTextBlock.Visibility = Visibility.Visible;
                // Consider logging the full exception: System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                LoadingRing.IsActive = false;
            }
            // No explicit return needed for async Task method that completes successfully
        }

        private void MedicationsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Medication selectedMedication)
            {
               Frame.Navigate(typeof(MedicationDetailsPage), selectedMedication.Id);
            }
        }

        private void AddMedicationButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddMedicationPage));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}