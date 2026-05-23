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
    public class CaretakerViewModel : BaseViewModel
    {
        private ObservableCollection<Caretaker> _caretakers;
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<PatientCaretakerAssignment> _assignments;
        private Caretaker _selectedCaretaker;
        private Patient _selectedPatient;
        private PatientCaretakerAssignment _selectedAssignment;
        private string _searchText;

        public ObservableCollection<Caretaker> Caretakers
        {
            get => _caretakers;
            set => SetProperty(ref _caretakers, value);
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public ObservableCollection<PatientCaretakerAssignment> Assignments
        {
            get => _assignments;
            set => SetProperty(ref _assignments, value);
        }

        public Caretaker SelectedCaretaker
        {
            get => _selectedCaretaker;
            set => SetProperty(ref _selectedCaretaker, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set => SetProperty(ref _selectedPatient, value);
        }

        public PatientCaretakerAssignment SelectedAssignment
        {
            get => _selectedAssignment;
            set => SetProperty(ref _selectedAssignment, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                SearchCaretakers();
            }
        }

        public ICommand AddCaretakerCommand { get; }
        public ICommand UpdateCaretakerCommand { get; }
        public ICommand DeleteCaretakerCommand { get; }
        public ICommand AssignPatientCommand { get; }
        public ICommand EndAssignmentCommand { get; }
        public ICommand RefreshCommand { get; }

        public CaretakerViewModel()
        {
            AddCaretakerCommand = new RelayCommand(param => AddCaretaker());
            UpdateCaretakerCommand = new RelayCommand(param => UpdateCaretaker(), param => SelectedCaretaker != null);
            DeleteCaretakerCommand = new RelayCommand(param => DeleteCaretaker(), param => SelectedCaretaker != null);
            AssignPatientCommand = new RelayCommand(param => AssignPatient());
            EndAssignmentCommand = new RelayCommand(param => EndAssignment(), param => SelectedAssignment != null);
            RefreshCommand = new RelayCommand(param => LoadData());

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var caretakers = CaretakerService.GetAllCaretakers();
                Caretakers = new ObservableCollection<Caretaker>(caretakers);

                var patients = PatientService.GetAllPatients();
                Patients = new ObservableCollection<Patient>(patients);

                var assignments = CaretakerService.GetAssignments();
                Assignments = new ObservableCollection<PatientCaretakerAssignment>(assignments);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCaretaker()
        {
            try
            {
                // Open Add Caretaker dialog
                var dialog = new AddCaretakerWindow();
                dialog.Owner = Application.Current.MainWindow;

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Refresh the list after adding
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Caretaker dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCaretaker()
        {
            if (SelectedCaretaker == null) return;

            MessageBox.Show("Edit Caretaker dialog would open here. You can implement dialogs using additional windows.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteCaretaker()
        {
            if (SelectedCaretaker == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedCaretaker.FullName}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    CaretakerService.DeleteCaretaker(SelectedCaretaker.CaretakerId);
                    MessageBox.Show("Caretaker deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting caretaker: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AssignPatient()
        {
            try
            {
                // Open Assign Patient dialog
                var dialog = new AssignPatientWindow();
                dialog.Owner = Application.Current.MainWindow;

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Refresh the assignments list
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Assign Patient dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EndAssignment()
        {
            if (SelectedAssignment == null) return;

            var result = MessageBox.Show("Are you sure you want to end this assignment?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    CaretakerService.EndAssignment(SelectedAssignment.AssignmentId, DateTime.Now);
                    MessageBox.Show("Assignment ended successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error ending assignment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchCaretakers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadData();
            }
            else
            {
                var allCaretakers = CaretakerService.GetAllCaretakers();
                var filtered = allCaretakers.Where(c =>
                    c.FirstName.ToLower().Contains(SearchText.ToLower()) ||
                    c.LastName.ToLower().Contains(SearchText.ToLower())).ToList();
                Caretakers = new ObservableCollection<Caretaker>(filtered);
            }
        }
    }
}
