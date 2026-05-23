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
    public class AddDoctorViewModel : BaseViewModel
    {
        private string _firstName;
        private string _lastName;
        private string _selectedSpecialization;
        private string _licenseNumber;
        private string _phoneNumber;
        private string _email;
        private decimal _consultationFee;
        private DateTime _joinDate;
        private bool _isAvailable;
        private string _errorMessage;
        private bool _hasError;

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string SelectedSpecialization
        {
            get => _selectedSpecialization;
            set => SetProperty(ref _selectedSpecialization, value);
        }

        public string LicenseNumber
        {
            get => _licenseNumber;
            set => SetProperty(ref _licenseNumber, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public decimal ConsultationFee
        {
            get => _consultationFee;
            set => SetProperty(ref _consultationFee, value);
        }

        public DateTime JoinDate
        {
            get => _joinDate;
            set => SetProperty(ref _joinDate, value);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
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

        public ObservableCollection<string> Specializations { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddDoctorViewModel()
        {
            // Initialize specializations
            Specializations = new ObservableCollection<string>
            {
                "General Medicine",
                "Cardiology",
                "Neurology",
                "Orthopedics",
                "Pediatrics",
                "Dermatology",
                "Psychiatry",
                "Ophthalmology",
                "ENT",
                "Gynecology",
                "Urology",
                "Radiology",
                "Anesthesiology",
                "Emergency Medicine"
            };

            // Set defaults
            JoinDate = DateTime.Now;
            IsAvailable = true;
            SelectedSpecialization = Specializations.FirstOrDefault();

            // Initialize commands
            SaveCommand = new RelayCommand(param => Save(), param => CanSave());
            CancelCommand = new RelayCommand(param => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(LicenseNumber) &&
                   !string.IsNullOrWhiteSpace(PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(SelectedSpecialization) &&
                   ConsultationFee > 0;
        }

        private void Save()
        {
            ErrorMessage = string.Empty;

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    ErrorMessage = "First name is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(LastName))
                {
                    ErrorMessage = "Last name is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(LicenseNumber))
                {
                    ErrorMessage = "License number is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    ErrorMessage = "Phone number is required";
                    return;
                }

                if (ConsultationFee <= 0)
                {
                    ErrorMessage = "Consultation fee must be greater than 0";
                    return;
                }

                // Create doctor object
                var doctor = new Doctor
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Specialization = SelectedSpecialization,
                    LicenseNumber = LicenseNumber.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    Email = Email?.Trim(),
                    ConsultationFee = ConsultationFee,
                    //JoinDate = JoinDate,
                    IsAvailable = IsAvailable
                };

                // Save to database
                DoctorService.AddDoctor(doctor);

                // Show success message
                MessageBox.Show("Doctor added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Close window
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving doctor: {ex.Message}";
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
