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

namespace EMR.ViewModels
{
    public class AssignPatientViewModel : BaseViewModel
    {
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Caretaker> _caretakers;
        private Patient _selectedPatient;
        private Caretaker _selectedCaretaker;
        private DateTime _assignedDate;
        private DateTime? _endDate;
        private string _notes;
        private string _errorMessage;
        private bool _hasError;
        private bool _showPatientInfo;
        private bool _showCaretakerInfo;

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public ObservableCollection<Caretaker> Caretakers
        {
            get => _caretakers;
            set => SetProperty(ref _caretakers, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                SetProperty(ref _selectedPatient, value);
                ShowPatientInfo = value != null;
            }
        }

        public Caretaker SelectedCaretaker
        {
            get => _selectedCaretaker;
            set
            {
                SetProperty(ref _selectedCaretaker, value);
                ShowCaretakerInfo = value != null;
            }
        }

        public DateTime AssignedDate
        {
            get => _assignedDate;
            set => SetProperty(ref _assignedDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public bool ShowPatientInfo
        {
            get => _showPatientInfo;
            set => SetProperty(ref _showPatientInfo, value);
        }

        public bool ShowCaretakerInfo
        {
            get => _showCaretakerInfo;
            set => SetProperty(ref _showCaretakerInfo, value);
        }

        public ICommand AssignCommand { get; }

        public ICommand CancelCommand { get; }

        public AssignPatientViewModel()
        {
            // Set default date
            AssignedDate = DateTime.Now;

            //EndDate = DateTime.Now;

            // Initialize commands
            AssignCommand = new RelayCommand(param => Assign(), param => CanAssign());
            CancelCommand = new RelayCommand(param => Cancel());

            // Load data
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Load patients
                var patientsList = PatientService.GetAllPatients();
                Patients = new ObservableCollection<Patient>(patientsList);

                // Load caretakers
                var caretakersList = CaretakerService.GetAllCaretakers();
                Caretakers = new ObservableCollection<Caretaker>(caretakersList);

                // Set defaults if available
                if (Patients.Count > 0)
                    SelectedPatient = Patients.FirstOrDefault();

                if (Caretakers.Count > 0)
                    SelectedCaretaker = Caretakers.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private bool CanAssign()
        {
            return SelectedPatient != null && SelectedCaretaker != null;
        }

        private void Assign()
        {
            ErrorMessage = string.Empty;

            try
            {
                // Validation
                if (SelectedPatient == null)
                {
                    ErrorMessage = "Please select a patient";
                    return;
                }

                if (SelectedCaretaker == null)
                {
                    ErrorMessage = "Please select a caretaker";
                    return;
                }

                // Create assignment
                var assignment = new PatientCaretakerAssignment
                {
                    PatientId = SelectedPatient.PatientId,
                    CaretakerId = SelectedCaretaker.CaretakerId,
                    AssignedDate = AssignedDate,
                    EndDate = EndDate,
                    Notes = Notes
                };

                // Save to database
                CaretakerService.AssignPatient(assignment);

                // Show success message
                MessageBox.Show(
                    $"Patient '{SelectedPatient.FullName}' has been successfully assigned to '{SelectedCaretaker.FullName}'!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Close window
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating assignment: {ex.Message}";
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
            // Find and close the window
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }
    }
}
