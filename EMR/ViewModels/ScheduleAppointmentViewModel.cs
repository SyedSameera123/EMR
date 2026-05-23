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
    public class ScheduleAppointmentViewModel : BaseViewModel
    {
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Doctor> _doctors;
        private Patient _selectedPatient;
        private Doctor _selectedDoctor;
        private DateTime _appointmentDate;
        private string _selectedTimeSlot;
        private string _selectedAppointmentType;
        private string _reason;
        private string _errorMessage;
        private bool _hasError;
        private bool _showPatientInfo;
        private bool _showDoctorInfo;

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

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
                SetProperty(ref _selectedPatient, value);
                ShowPatientInfo = value != null;
            }
        }

        public Doctor SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                SetProperty(ref _selectedDoctor, value);
                ShowDoctorInfo = value != null;
            }
        }

        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set => SetProperty(ref _appointmentDate, value);
        }

        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set => SetProperty(ref _selectedTimeSlot, value);
        }

        public string SelectedAppointmentType
        {
            get => _selectedAppointmentType;
            set => SetProperty(ref _selectedAppointmentType, value);
        }

        public string Reason
        {
            get => _reason;
            set => SetProperty(ref _reason, value);
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

        public bool ShowDoctorInfo
        {
            get => _showDoctorInfo;
            set => SetProperty(ref _showDoctorInfo, value);
        }

        public ObservableCollection<string> TimeSlots { get; }
        public ObservableCollection<string> AppointmentTypes { get; }

        public ICommand ScheduleCommand { get; }
        public ICommand CancelCommand { get; }

        public ScheduleAppointmentViewModel()
        {
            // Initialize time slots
            TimeSlots = new ObservableCollection<string>
            {
                "09:00 AM", "09:30 AM", "10:00 AM", "10:30 AM",
                "11:00 AM", "11:30 AM", "12:00 PM", "12:30 PM",
                "02:00 PM", "02:30 PM", "03:00 PM", "03:30 PM",
                "04:00 PM", "04:30 PM", "05:00 PM", "05:30 PM"
            };

            // Initialize appointment types
            AppointmentTypes = new ObservableCollection<string>
            {
                "Regular Checkup",
                "Follow-up",
                "Emergency",
                "Consultation",
                "Treatment",
                "Surgery"
            };

            // Set defaults
            AppointmentDate = DateTime.Now;
            SelectedTimeSlot = TimeSlots.FirstOrDefault();
            SelectedAppointmentType = AppointmentTypes.FirstOrDefault();

            // Initialize commands
            ScheduleCommand = new RelayCommand(param => Schedule(), param => CanSchedule());
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

                // Load doctors
                var doctorsList = DoctorService.GetAllDoctors();
                Doctors = new ObservableCollection<Doctor>(doctorsList);

                // Set defaults if available
                if (Patients.Count > 0)
                    SelectedPatient = Patients.FirstOrDefault();

                if (Doctors.Count > 0)
                    SelectedDoctor = Doctors.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private bool CanSchedule()
        {
            return SelectedPatient != null &&
                   SelectedDoctor != null &&
                   !string.IsNullOrWhiteSpace(SelectedTimeSlot) &&
                   !string.IsNullOrWhiteSpace(Reason);
        }

        private void Schedule()
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

                if (SelectedDoctor == null)
                {
                    ErrorMessage = "Please select a doctor";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Reason))
                {
                    ErrorMessage = "Please enter reason for visit";
                    return;
                }

                // Combine date and time
                DateTime appointmentDateTime = AppointmentDate.Date;
                if (!string.IsNullOrWhiteSpace(SelectedTimeSlot))
                {
                    var timeParts = SelectedTimeSlot.Split(' ');
                    var time = TimeSpan.Parse(timeParts[0]);
                    if (timeParts[1] == "PM" && time.Hours < 12)
                    {
                        time = time.Add(TimeSpan.FromHours(12));
                    }
                    appointmentDateTime = appointmentDateTime.Add(time);
                }

                // Create appointment
                var appointment = new Appointment
                {
                    PatientId = SelectedPatient.PatientId,
                    DoctorId = SelectedDoctor.DoctorId,
                    AppointmentDate = appointmentDateTime,
                    AppointmentType = SelectedAppointmentType,
                    Reason = Reason.Trim(),
                    Status = "Scheduled"
                };

                // Save to database
                AppointmentService.AddAppointment(appointment);

                // Show success message
                MessageBox.Show(
                    $"Appointment scheduled successfully!\n\n" +
                    $"Patient: {SelectedPatient.FullName}\n" +
                    $"Doctor: {SelectedDoctor.FullName}\n" +
                    $"Date: {appointmentDateTime:dddd, MMMM dd, yyyy}\n" +
                    $"Time: {SelectedTimeSlot}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Close window
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error scheduling appointment: {ex.Message}";
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
