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
    public class AddPatientViewModel : BaseViewModel
    {
        private string _firstName;
        private string _lastName;
        private DateTime _dateOfBirth;
        private string _selectedGender;
        private string _selectedBloodGroup;
        private string _phoneNumber;
        private string _email;
        private string _address;
        private string _city;
        private string _state;
        private string _emergencyContactName;
        private string _emergencyContactPhone;
        private string _selectedRelationship;
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

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public string SelectedGender
        {
            get => _selectedGender;
            set => SetProperty(ref _selectedGender, value);
        }

        public string SelectedBloodGroup
        {
            get => _selectedBloodGroup;
            set => SetProperty(ref _selectedBloodGroup, value);
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

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public string EmergencyContactName
        {
            get => _emergencyContactName;
            set => SetProperty(ref _emergencyContactName, value);
        }

        public string EmergencyContactPhone
        {
            get => _emergencyContactPhone;
            set => SetProperty(ref _emergencyContactPhone, value);
        }

        public string SelectedRelationship
        {
            get => _selectedRelationship;
            set => SetProperty(ref _selectedRelationship, value);
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

        public ObservableCollection<string> Genders { get; }
        public ObservableCollection<string> BloodGroups { get; }
        public ObservableCollection<string> Relationships { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddPatientViewModel()
        {
            // Initialize genders
            Genders = new ObservableCollection<string>
            {
                "Male",
                "Female",
                "Other"
            };

            // Initialize blood groups
            BloodGroups = new ObservableCollection<string>
            {
                "A+", "A-",
                "B+", "B-",
                "AB+", "AB-",
                "O+", "O-"
            };

            // Initialize relationships
            Relationships = new ObservableCollection<string>
            {
                "Parent",
                "Spouse",
                "Sibling",
                "Child",
                "Friend",
                "Relative",
                "Other"
            };

            // Set defaults
            DateOfBirth = DateTime.Now.AddYears(-30); // Default 30 years old
            SelectedGender = Genders.FirstOrDefault();
            SelectedBloodGroup = BloodGroups.FirstOrDefault();
            SelectedRelationship = Relationships.FirstOrDefault();

            // Initialize commands
            SaveCommand = new RelayCommand(param => Save(), param => CanSave());
            CancelCommand = new RelayCommand(param => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(PhoneNumber) &&
                   !string.IsNullOrWhiteSpace(Address) &&
                   !string.IsNullOrWhiteSpace(City) &&
                   !string.IsNullOrWhiteSpace(SelectedGender) &&
                   !string.IsNullOrWhiteSpace(SelectedBloodGroup);
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

                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    ErrorMessage = "Phone number is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Address))
                {
                    ErrorMessage = "Address is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(City))
                {
                    ErrorMessage = "City is required";
                    return;
                }

                if (DateOfBirth >= DateTime.Now)
                {
                    ErrorMessage = "Please select a valid date of birth in the past";
                    return;
                }

                // Calculate age - CORRECTED LOGIC
                int age = CalculateAge(DateOfBirth);

                if (age < 0 || age > 150)
                {
                    ErrorMessage = "Invalid date of birth. Age must be between 0 and 150 years";
                    return;
                }

                // Create patient object
                var patient = new Patient
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    DateOfBirth = DateOfBirth,
                    Age = age,  // â† Calculated age
                    Gender = SelectedGender,
                    BloodGroup = SelectedBloodGroup,
                    PhoneNumber = PhoneNumber.Trim(),
                    Email = Email?.Trim(),
                    Address = Address?.Trim(),
                    City = City?.Trim(),
                    State = State?.Trim(),
                    EmergencyContactName = EmergencyContactName?.Trim(),
                    EmergencyContactPhone = EmergencyContactPhone?.Trim(),
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };

                // Save to database
                PatientService.AddPatient(patient);

                // Show success message with age
                MessageBox.Show(
                    $"Patient registered successfully!\n\n" +
                    $"Name: {patient.FirstName} {patient.LastName}\n" +
                    $"Age: {patient.Age} years\n" +
                    $"Date of Birth: {patient.DateOfBirth:dd-MMM-yyyy}\n" +
                    $"Blood Group: {patient.BloodGroup}\n" +
                    $"Phone: {patient.PhoneNumber}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Close window
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving patient: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Save error: {ex}");
            }
        }

        // CORRECTED AGE CALCULATION METHOD
        private int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;

            // Subtract one year if birthday hasn't occurred this year yet
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
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
