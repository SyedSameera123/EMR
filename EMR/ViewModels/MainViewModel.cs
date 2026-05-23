using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EMR.Commands;
using EMR.Services;
using System.Windows.Input;
using System.Windows;
using EMR.Views;

namespace EMR.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;
        private string _currentModule = "Caretaker";
        private string _welcomeMessage;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string CurrentModule
        {
            get => _currentModule;
            set => SetProperty(ref _currentModule, value);
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => SetProperty(ref _welcomeMessage, value);
        }

        public ICommand ShowCaretakerCommand { get; }
        public ICommand ShowAmbulatoryCommand { get; }
        public ICommand ShowBedManagementCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            ShowCaretakerCommand = new RelayCommand(_ => ShowCaretaker());
            ShowAmbulatoryCommand = new RelayCommand(_ => ShowAmbulatory());
            ShowBedManagementCommand = new RelayCommand(_ => ShowBedManagement());
            LogoutCommand = new RelayCommand(_ => Logout());

            // Set welcome message
            if (AuthenticationService.CurrentUser != null)
            {
                WelcomeMessage = $"Welcome, {AuthenticationService.CurrentUser.FullName} ({AuthenticationService.CurrentUser.RoleName})";
            }

            // Show default view
            ShowCaretaker();
        }

        private void ShowCaretaker()
        {
            CurrentModule = "Caretaker";
            CurrentView = new CaretakerViewModel();
        }

        private void ShowAmbulatory()
        {
            CurrentModule = "Ambulatory Services";
            CurrentView = new AmbulatoryViewModel();
        }

        private void ShowBedManagement()
        {
            CurrentModule = "Bed Management";
            CurrentView = new BedManagementViewModel();
        }

        private void Logout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AuthenticationService.Logout();

                // Open login window
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();

                // Close main window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }
    }
}
