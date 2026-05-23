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
    public class AmbulatoryViewModel : BaseViewModel
    {
        private ObservableCollection<Doctor> _doctors;
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Appointment> _appointments;
        private Doctor _selectedDoctor;
        private Patient _selectedPatient;
        private Appointment _selectedAppointment;

        public ObservableCollection<Doctor> Doctors
        {
            get => _doctors;
            set => SetProperty(ref _doctors, value);
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public ObservableCollection<Appointment> Appointments
        {
            get => _appointments;
            set => SetProperty(ref _appointments, value);
        }

        public Doctor SelectedDoctor
        {
            get => _selectedDoctor;
            set => SetProperty(ref _selectedDoctor, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set => SetProperty(ref _selectedPatient, value);
        }

        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                if (SetProperty(ref _selectedAppointment, value))
                {
                    // Force command re-evaluation when selection changes
                    CommandManager.InvalidateRequerySuggested();

                    // Debug output
                    if (value != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Selected appointment: ID={value.AppointmentId}, Status={value.Status}");
                        System.Diagnostics.Debug.WriteLine($"Can Complete: {CanCompleteAppointment()}");
                        System.Diagnostics.Debug.WriteLine($"Can Cancel: {CanCancelAppointment()}");
                    }
                }
            }
        }

        public ICommand AddDoctorCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand UpdateAppointmentCommand { get; }
        public ICommand CompleteAppointmentCommand { get; }
        public ICommand CancelAppointmentCommand { get; }
        public ICommand RefreshCommand { get; }

        public AmbulatoryViewModel()
        {
            AddDoctorCommand = new RelayCommand(param => AddDoctor());
            AddPatientCommand = new RelayCommand(param => AddPatient());
            AddAppointmentCommand = new RelayCommand(param => AddAppointment());
            UpdateAppointmentCommand = new RelayCommand(param => UpdateAppointment(), param => CanUpdateAppointment());

            // CRITICAL: Use method references for CanExecute
            CompleteAppointmentCommand = new RelayCommand(
                param => CompleteAppointment(),
                param => CanCompleteAppointment());

            CancelAppointmentCommand = new RelayCommand(
                param => CancelAppointment(),
                param => CanCancelAppointment());

            RefreshCommand = new RelayCommand(param => LoadData());

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var doctors = DoctorService.GetAllDoctors();
                Doctors = new ObservableCollection<Doctor>(doctors);

                var patients = PatientService.GetAllPatients();
                Patients = new ObservableCollection<Patient>(patients);

                var appointments = AppointmentService.GetAllAppointments();
                Appointments = new ObservableCollection<Appointment>(appointments);

                System.Diagnostics.Debug.WriteLine($"Loaded {Appointments.Count} appointments");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"LoadData error: {ex}");
            }
        }

        private void AddDoctor()
        {
            try
            {
                var dialog = new AddDoctorWindow();
                dialog.Owner = Application.Current.MainWindow;

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Doctor dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPatient()
        {
            try
            {
                var dialog = new AddPatientWindow();
                dialog.Owner = Application.Current.MainWindow;

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Patient dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAppointment()
        {
            try
            {
                var dialog = new ScheduleAppointmentWindow();
                dialog.Owner = Application.Current.MainWindow;

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Schedule Appointment dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUpdateAppointment()
        {
            return SelectedAppointment != null;
        }

        private void UpdateAppointment()
        {
            if (SelectedAppointment == null) return;

            MessageBox.Show("Edit Appointment functionality coming soon.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanCompleteAppointment()
        {
            if (SelectedAppointment == null)
            {
                System.Diagnostics.Debug.WriteLine("CanComplete: No appointment selected");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedAppointment.Status))
            {
                System.Diagnostics.Debug.WriteLine("CanComplete: Status is null or empty");
                return false;
            }

            bool isScheduled = SelectedAppointment.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase);
            System.Diagnostics.Debug.WriteLine($"CanComplete: Status='{SelectedAppointment.Status}', IsScheduled={isScheduled}");

            return isScheduled;
        }

        private void CompleteAppointment()
        {
            if (SelectedAppointment == null) return;

            var result = MessageBox.Show(
                $"Mark this appointment as Completed?\n\n" +
                $"Patient: {SelectedAppointment.PatientName}\n" +
                $"Doctor: {SelectedAppointment.DoctorName}\n" +
                $"Date: {SelectedAppointment.AppointmentDate:g}\n" +
                $"Type: {SelectedAppointment.AppointmentType}",
                "Confirm Complete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppointmentService.UpdateAppointmentStatus(
                        SelectedAppointment.AppointmentId,
                        "Completed");

                    MessageBox.Show("Appointment marked as Completed!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error completing appointment: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine($"CompleteAppointment error: {ex}");
                }
            }
        }

        private bool CanCancelAppointment()
        {
            if (SelectedAppointment == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(SelectedAppointment.Status))
            {
                return false;
            }

            return SelectedAppointment.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase);
        }

        private void CancelAppointment()
        {
            if (SelectedAppointment == null) return;

            var result = MessageBox.Show(
                $"Cancel this appointment?\n\n" +
                $"Patient: {SelectedAppointment.PatientName}\n" +
                $"Doctor: {SelectedAppointment.DoctorName}\n" +
                $"Date: {SelectedAppointment.AppointmentDate:g}\n" +
                $"Type: {SelectedAppointment.AppointmentType}",
                "Confirm Cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AppointmentService.UpdateAppointmentStatus(
                        SelectedAppointment.AppointmentId,
                        "Cancelled");

                    MessageBox.Show("Appointment cancelled!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cancelling appointment: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine($"CancelAppointment error: {ex}");
                }
            }
        }
    }
}
