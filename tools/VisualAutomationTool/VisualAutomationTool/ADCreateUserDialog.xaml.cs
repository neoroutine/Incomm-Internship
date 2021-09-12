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
    /// Logique d'interaction pour ADCreateUser.xaml
    /// </summary>
    public partial class ADCreateUserDialog : Window
    {
        public ADCreateUserDialog(List<string> availableServices)
        {
            InitializeComponent();
            this.DataContext = this;

            this.AvailableServices = availableServices;

            this.ServicesCombo.Focus();

        }

        public List<string> AvailableServices { get; set; }

        private void CreateUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Prenom.Text.Length > 0 && this.Nom.Text.Length > 0)
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
