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
    public class AddCaretakerViewModel : BaseViewModel
    {
        private string _firstName;
        private string _lastName;
        private string _selectedSpecialization;
        private string _licenseNumber;
        private string _phoneNumber;
        private string _selectedShift;
        private DateTime _joinDate;
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

        public string SelectedShift
        {
            get => _selectedShift;
            set => SetProperty(ref _selectedShift, value);
        }

        public DateTime JoinDate
        {
            get => _joinDate;
            set => SetProperty(ref _joinDate, value);
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
        public ObservableCollection<string> Shifts { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddCaretakerViewModel()
        {
            // Initialize collections
            Specializations = new ObservableCollection<string>
            {
                "General Care",
                "Pediatric Care",
                "Geriatric Care",
                "ICU Care",
                "Emergency Care",
                "Post-Operative Care",
                "Palliative Care"
            };

            Shifts = new ObservableCollection<string>
            {
                "Morning",
                "Evening",
                "Night"
            };

            // Set defaults
            JoinDate = DateTime.Now;
            SelectedSpecialization = Specializations.FirstOrDefault();
            SelectedShift = Shifts.FirstOrDefault();

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
                   !string.IsNullOrWhiteSpace(SelectedShift);
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

                // Create caretaker object
                var caretaker = new Caretaker
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Specialization = SelectedSpecialization,
                    LicenseNumber = LicenseNumber.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    Shift = SelectedShift,
                    JoinDate = JoinDate,
                    IsActive = true
                };

                // Save to database
                CaretakerService.AddCaretaker(caretaker);

                // Show success message
                MessageBox.Show("Caretaker added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Close window
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving caretaker: {ex.Message}";
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
