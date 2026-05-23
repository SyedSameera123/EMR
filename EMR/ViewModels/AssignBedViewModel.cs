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
    public class AssignBedViewModel : BaseViewModel
    {
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Doctor> _doctors;  // ← NEW
        private Patient _selectedPatient;
        private Doctor _selectedDoctor;  // ← NEW
        private Bed _selectedBed;
        private DateTime _admissionDate;
        private DateTime? _expectedDischargeDate;
        private string _admissionReason;
        private string _errorMessage;
        private bool _hasError;
        private bool _showPatientInfo;
        private bool _showBedInfo;

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        // ← NEW: Doctors collection
        public ObservableCollection<Doctor> Doctors
        {
            get => _doctors;
            set => SetProperty(ref _doctors, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    ShowPatientInfo = value != null;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // ← NEW: Selected doctor
        public Doctor SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                if (SetProperty(ref _selectedDoctor, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Bed SelectedBed
        {
            get => _selectedBed;
            set
            {
                SetProperty(ref _selectedBed, value);
                ShowBedInfo = value != null;
            }
        }

        public DateTime AdmissionDate
        {
            get => _admissionDate;
            set => SetProperty(ref _admissionDate, value);
        }

        public DateTime? ExpectedDischargeDate
        {
            get => _expectedDischargeDate;
            set => SetProperty(ref _expectedDischargeDate, value);
        }

        public string AdmissionReason
        {
            get => _admissionReason;
            set
            {
                if (SetProperty(ref _admissionReason, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
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

        public bool ShowBedInfo
        {
            get => _showBedInfo;
            set => SetProperty(ref _showBedInfo, value);
        }

        public ICommand AssignCommand { get; }
        public ICommand CancelCommand { get; }

        public AssignBedViewModel()
        {
            AdmissionDate = DateTime.Now;

            AssignCommand = new RelayCommand(
                param => Assign(),
                param => CanAssign());

            CancelCommand = new RelayCommand(param => Cancel());

            LoadPatients();
            LoadDoctors();  // ← NEW: Load doctors
        }

        private void LoadPatients()
        {
            try
            {
                var patientsList = PatientService.GetAllPatients();
                Patients = new ObservableCollection<Patient>(patientsList);

                if (Patients.Count > 0)
                    SelectedPatient = Patients.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading patients: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadPatients error: {ex}");
            }
        }

        // ← NEW: Load doctors method
        private void LoadDoctors()
        {
            try
            {
                var doctorsList = BedService.GetAllDoctors();
                Doctors = new ObservableCollection<Doctor>(doctorsList);

                if (Doctors.Count > 0)
                    SelectedDoctor = Doctors.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading doctors: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadDoctors error: {ex}");
            }
        }

        private bool CanAssign()
        {
            bool canAssign = SelectedPatient != null &&
                            SelectedBed != null &&
                            SelectedDoctor != null &&  // ← NEW: Check doctor is selected
                            !string.IsNullOrWhiteSpace(AdmissionReason);

            return canAssign;
        }

        private void Assign()
        {
            ErrorMessage = string.Empty;

            try
            {
                if (SelectedPatient == null)
                {
                    ErrorMessage = "Please select a patient";
                    return;
                }

                if (SelectedBed == null)
                {
                    ErrorMessage = "No bed selected";
                    return;
                }

                // ← NEW: Validate doctor selection
                if (SelectedDoctor == null)
                {
                    ErrorMessage = "Please select a doctor";
                    return;
                }

                if (string.IsNullOrWhiteSpace(AdmissionReason))
                {
                    ErrorMessage = "Please enter admission reason";
                    return;
                }

                var assignment = new BedAssignment
                {
                    BedId = SelectedBed.BedId,
                    PatientId = SelectedPatient.PatientId,
                    DoctorId = SelectedDoctor.DoctorId,  // ← NEW: Include DoctorId
                    AdmissionDate = AdmissionDate,
                    ExpectedDischargeDate = ExpectedDischargeDate,
                    AdmissionReason = AdmissionReason.Trim(),
                    Reason = AdmissionReason.Trim()  // For backward compatibility
                };

                BedService.AssignBed(assignment);

                MessageBox.Show(
                    $"Bed assigned successfully!\n\n" +
                    $"Patient: {SelectedPatient.FullName}\n" +
                    $"Doctor: Dr. {SelectedDoctor.FirstName} {SelectedDoctor.LastName}\n" +  // ← NEW
                    $"Bed: {SelectedBed.BedNumber}\n" +
                    $"Ward: {SelectedBed.WardName}\n" +
                    $"Admission Date: {AdmissionDate:g}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                CloseWindow(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error assigning bed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Assign error: {ex}");
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool dialogResult)
        {
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