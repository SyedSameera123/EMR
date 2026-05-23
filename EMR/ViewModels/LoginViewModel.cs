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
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isLoginMode = true;

        // Registration fields
        private string _regUsername;
        private string _regPassword;
        private string _regConfirmPassword;
        private string _regEmail;
        private string _regFirstName;
        private string _regLastName;
        private string _regPhoneNumber;
        private Role _selectedRole;
        private ObservableCollection<Role> _roles;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set => SetProperty(ref _isLoginMode, value);
        }

        public string RegUsername
        {
            get => _regUsername;
            set => SetProperty(ref _regUsername, value);
        }

        public string RegPassword
        {
            get => _regPassword;
            set => SetProperty(ref _regPassword, value);
        }

        public string RegConfirmPassword
        {
            get => _regConfirmPassword;
            set => SetProperty(ref _regConfirmPassword, value);
        }

        public string RegEmail
        {
            get => _regEmail;
            set => SetProperty(ref _regEmail, value);
        }

        public string RegFirstName
        {
            get => _regFirstName;
            set => SetProperty(ref _regFirstName, value);
        }

        public string RegLastName
        {
            get => _regLastName;
            set => SetProperty(ref _regLastName, value);
        }

        public string RegPhoneNumber
        {
            get => _regPhoneNumber;
            set => SetProperty(ref _regPhoneNumber, value);
        }

        public Role SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand SwitchToRegisterCommand { get; }
        public ICommand SwitchToLoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            SwitchToRegisterCommand = new RelayCommand(_ => SwitchToRegister());
            SwitchToLoginCommand = new RelayCommand(_ => SwitchToLogin());

            LoadRoles();
        }

        private void LoadRoles()
        {
            try
            {
                var roles = RoleService.GetAllRoles();
                Roles = new ObservableCollection<Role>(roles);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load roles: {ex.Message}";
            }
        }

        private void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter username and password";
                return;
            }

            try
            {
                var user = AuthenticationService.Login(Username, Password);  // Changed from bool to User

                if (user != null)  // Changed from if (success)
                {
                    // Open main window
                    var mainWindow = new MainWindow();
                    mainWindow.Show();

                    // Close login window
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
        }

        private void ExecuteRegister(object parameter)
        {
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(RegUsername) || string.IsNullOrWhiteSpace(RegPassword) ||
                string.IsNullOrWhiteSpace(RegEmail) || string.IsNullOrWhiteSpace(RegFirstName) ||
                string.IsNullOrWhiteSpace(RegLastName))
            {
                ErrorMessage = "Please fill in all required fields";
                return;
            }

            if (RegPassword != RegConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            if (RegPassword.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                return;
            }

            if (SelectedRole == null)
            {
                ErrorMessage = "Please select a role";
                return;
            }

            try
            {
                // Check if username is available
                if (!AuthenticationService.IsUsernameAvailable(RegUsername))
                {
                    ErrorMessage = "Username already exists";
                    return;
                }

                // Check if email is available
                if (!AuthenticationService.IsEmailAvailable(RegEmail))
                {
                    ErrorMessage = "Email already registered";
                    return;
                }

                bool success = AuthenticationService.Register(
                    RegUsername, RegPassword, RegEmail, RegFirstName,
                    RegLastName, SelectedRole.RoleId, RegPhoneNumber);

                if (success)
                {
                    MessageBox.Show("Registration successful! You can now login.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    SwitchToLogin();
                    ClearRegistrationFields();
                }
                else
                {
                    ErrorMessage = "Registration failed. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
            }
        }

        private void SwitchToRegister()
        {
            IsLoginMode = false;
            ErrorMessage = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }

        private void SwitchToLogin()
        {
            IsLoginMode = true;
            ErrorMessage = string.Empty;
            ClearRegistrationFields();
        }

        private void ClearRegistrationFields()
        {
            RegUsername = string.Empty;
            RegPassword = string.Empty;
            RegConfirmPassword = string.Empty;
            RegEmail = string.Empty;
            RegFirstName = string.Empty;
            RegLastName = string.Empty;
            RegPhoneNumber = string.Empty;
            SelectedRole = null;
        }
    }
}
