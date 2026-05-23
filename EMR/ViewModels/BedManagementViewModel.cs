using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMR.Commands;
using EMR.Models;
using EMR.Services;
using System.Windows.Input;
using System.Windows;
using EMR.Views;

namespace EMR.ViewModels
{
    public class BedManagementViewModel : BaseViewModel
    {
        private ObservableCollection<Ward> _wards;
        private ObservableCollection<Bed> _beds;
        private ObservableCollection<BedAssignment> _assignments;
        private Ward _selectedWard;
        private Bed _selectedBed;
        private BedAssignment _selectedAssignment;
        private int _totalBeds;
        private int _availableBeds;
        private int _occupiedBeds;
        private double _occupancyRate;

        public ObservableCollection<Ward> Wards
        {
            get => _wards;
            set => SetProperty(ref _wards, value);
        }

        public ObservableCollection<Bed> Beds
        {
            get => _beds;
            set => SetProperty(ref _beds, value);
        }

        public ObservableCollection<BedAssignment> Assignments
        {
            get => _assignments;
            set => SetProperty(ref _assignments, value);
        }

        public Ward SelectedWard
        {
            get => _selectedWard;
            set
            {
                SetProperty(ref _selectedWard, value);
                LoadBedsByWard();
            }
        }

        public Bed SelectedBed
        {
            get => _selectedBed;
            set => SetProperty(ref _selectedBed, value);
        }

        public BedAssignment SelectedAssignment
        {
            get => _selectedAssignment;
            set => SetProperty(ref _selectedAssignment, value);
        }

        public int TotalBeds
        {
            get => _totalBeds;
            set => SetProperty(ref _totalBeds, value);
        }

        public int AvailableBeds
        {
            get => _availableBeds;
            set => SetProperty(ref _availableBeds, value);
        }

        public int OccupiedBeds
        {
            get => _occupiedBeds;
            set => SetProperty(ref _occupiedBeds, value);
        }

        public double OccupancyRate
        {
            get => _occupancyRate;
            set => SetProperty(ref _occupancyRate, value);
        }

        public ICommand AssignBedCommand { get; }
        public ICommand DischargeBedCommand { get; }
        public ICommand RefreshCommand { get; }

        public BedManagementViewModel()
        {
            AssignBedCommand = new RelayCommand(param => AssignBed(), param => CanAssignBed());
            DischargeBedCommand = new RelayCommand(param => DischargeBed(), param => CanDischarge());
            RefreshCommand = new RelayCommand(param => LoadData());

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Load wards
                var wardsList = BedService.GetAllWards();
                Wards = new ObservableCollection<Ward>(wardsList);

                // Load all beds
                var bedsList = BedService.GetAllBeds();
                Beds = new ObservableCollection<Bed>(bedsList);

                // Load active assignments
                var assignmentsList = BedService.GetActiveAssignments();
                Assignments = new ObservableCollection<BedAssignment>(assignmentsList);

                // Calculate statistics
                CalculateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nPlease check database connection.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBedsByWard()
        {
            if (SelectedWard == null)
            {
                // Load all beds if no ward selected
                var allBeds = BedService.GetAllBeds();
                Beds = new ObservableCollection<Bed>(allBeds);
            }
            else
            {
                // Load beds for selected ward
                var wardBeds = BedService.GetBedsByWard(SelectedWard.WardId);
                Beds = new ObservableCollection<Bed>(wardBeds);
            }
        }

        private void CalculateStatistics()
        {
            try
            {
                if (Wards != null && Wards.Count > 0)
                {
                    TotalBeds = Wards.Sum(w => w.TotalBeds);
                    AvailableBeds = Wards.Sum(w => w.AvailableBeds);
                    OccupiedBeds = TotalBeds - AvailableBeds;
                    OccupancyRate = TotalBeds > 0 ? (double)OccupiedBeds / TotalBeds * 100 : 0;
                }
                else
                {
                    TotalBeds = 0;
                    AvailableBeds = 0;
                    OccupiedBeds = 0;
                    OccupancyRate = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating statistics: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAssignBed()
        {
            return SelectedBed != null && SelectedBed.Status == "Available";
        }

        private bool CanDischarge()
        {
            return SelectedAssignment != null;
        }

        private void AssignBed()
        {
            if (SelectedBed == null || SelectedBed.Status != "Available")
            {
                MessageBox.Show("Please select an available bed", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Open Assign Bed dialog
                var dialog = new AssignBedWindow();
                dialog.Owner = Application.Current.MainWindow;

                // Pass selected bed to dialog
                if (dialog.DataContext is AssignBedViewModel viewModel)
                {
                    viewModel.SelectedBed = SelectedBed;
                }

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Refresh after assignment
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Assign Bed dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DischargeBed()
        {
            if (SelectedAssignment == null) return;

            var result = MessageBox.Show(
                $"Discharge this patient?\n\n" +
                $"Patient: {SelectedAssignment.PatientName}\n" +
                $"Bed: {SelectedAssignment.BedNumber}\n" +
                $"Ward: {SelectedAssignment.WardName}\n" +
                $"Admitted: {SelectedAssignment.AdmissionDate:g}",
                "Confirm Discharge",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Discharge patient (includes automatic billing)
                    BedService.DischargeBed(SelectedAssignment.AssignmentId, DateTime.Now);

                    MessageBox.Show(
                        "Patient discharged successfully!\n\n" +
                        "Billing has been automatically calculated and recorded.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error discharging patient: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

}
