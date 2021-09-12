using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VisualAutomationTool
{
    /// <summary>
    /// Logique d'interaction pour ADDeleteUserDialog.xaml
    /// </summary>
    public partial class ADDeleteUserDialog : Window
    {
        public ADDeleteUserDialog(List<string> availableServices, ActiveDirectoryManager adManager)
        {
            InitializeComponent();
            this.DataContext = this;

            this.AvailableServices = availableServices;
            this.ADManager = adManager;

            this.ServicesCombo.SelectedIndex = 0;
            this.ServicesCombo.Focus();
        }

        public List<string> AvailableServices { get; set; }

        public ActiveDirectoryManager ADManager { get; set; }

        private void ServicesComboSelectionChanged(object sender, RoutedEventArgs e)
        {
            string selectedService = ServicesCombo.SelectedItem.ToString();
            UsersCombo.ItemsSource = ADManager.GetAllUsersFromServiceAsList(selectedService);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                string selectedService = ServicesCombo.SelectedItem.ToString();


                foreach (string user in ADManager.GetAllUsersFromServiceAsList(selectedService))
                {
                    if (user.Contains(UserInput.Text))
                    {
                        UsersCombo.SelectedItem = user;
                    }

                }

            }
        }

        private void DeleteUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (UsersCombo.SelectedIndex != -1)
            {
                this.DialogResult = true;
            }
                
            else
            {
                this.Error.Content = "Veuillez remplir tous les champs avec des informations valides";
            }
        }
    }
}
