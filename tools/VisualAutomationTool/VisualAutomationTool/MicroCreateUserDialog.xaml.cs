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
    /// Logique d'interaction pour MicroCreateUserDialog.xaml
    /// </summary>
    public partial class MicroCreateUserDialog : Window
    {
        public MicroCreateUserDialog(List<string> availableLicenses, int microRemainingAvailableBasicLicenses, int microRemainingAvailableStandardLicenses)
        {
            InitializeComponent();
            this.DataContext = this;

            this.AvailableLicenses = availableLicenses;
            LicensesCombo.SelectedIndex = 0;

            this.MicroRemainingAvailableBasicLicenses = microRemainingAvailableBasicLicenses;
            this.MicroRemainingAvailableStandardLicenses = microRemainingAvailableStandardLicenses;

            this.LabelRemainingAvailableBasicLicenses.Content = String.Format("{0} licences basic restantes", this.MicroRemainingAvailableBasicLicenses);
            this.LabelRemainingAvailableStandardLicenses.Content = String.Format("{0} licences standard restantes", this.MicroRemainingAvailableStandardLicenses);


            Prenom.Focus();
        }

        public List<string> AvailableLicenses { get; set; }

        public int MicroRemainingAvailableBasicLicenses { get; set; }
        public int MicroRemainingAvailableStandardLicenses { get; set; }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (Prenom.Text.Length > 0 && Nom.Text.Length > 0)
            {
                Mail.Text = Prenom.Text[0].ToString().ToLower() + Nom.Text.ToLower() + "@incomm.fr";
            }
            else
            {
                Mail.Text = "[Initiale Prénom][Nom]@incomm.fr";
            }
        }

        private void CreateUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (Prenom.Text.Length > 0 && Nom.Text.Length > 0)
            {
                this.DialogResult = true;
            }
            else if ((LicensesCombo.SelectedItem.ToString() == "Licence Basic" && MicroRemainingAvailableBasicLicenses == 0) ||
                     (LicensesCombo.SelectedItem.ToString() == "Licence Standard" && MicroRemainingAvailableStandardLicenses == 0))
            {
                Error.Content = "Veuillez libérer des licences basic/standard, il en manque";
            }
            else
            {
                this.Error.Content = "Veuillez remplir tous les champs avec des informations valides";
            }
        }
    }
}
