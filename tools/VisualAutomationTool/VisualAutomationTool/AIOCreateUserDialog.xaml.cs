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
    /// Logique d'interaction pour AIOCreateUserDialog.xaml
    /// </summary>
    public partial class AIOCreateUserDialog : Window
    {
        public AIOCreateUserDialog(List<string> llServices, List<string> llAgences, List<string> adServices, 
                                   List<string> microLicences, int microRemainingAvailableBasicLicenses, int microRemainingAvailableStandardLicenses)
        {
            InitializeComponent();
            this.DataContext = this;

            this.LLServices    = llServices;
            this.LLAgences     = llAgences;
            this.ADServices    = adServices;
            this.MicroLicences = microLicences;
            this.MicroRemainingAvailableBasicLicenses = microRemainingAvailableBasicLicenses;
            this.MicroRemainingAvailableStandardLicenses = microRemainingAvailableStandardLicenses;

            this.LabelRemainingAvailableBasicLicenses.Content = String.Format("{0} licences basic restantes", this.MicroRemainingAvailableBasicLicenses);
            this.LabelRemainingAvailableStandardLicenses.Content = String.Format("{0} licences standard restantes", this.MicroRemainingAvailableStandardLicenses);


            this.Prenom.Focus();
        }

        public List<string> LLServices { get; set; }
        public List<string> LLAgences { get; set; }

        public List<string> ADServices { get; set; }

        public List<string> MicroLicences { get; set; }

        public int MicroRemainingAvailableBasicLicenses { get; set; }
        public int MicroRemainingAvailableStandardLicenses { get; set; }

        private void RadioCustomTemplateClick(object sender, RoutedEventArgs e)
        {
            ComboLLServices.SelectedIndex = -1;
            CheckboxSuivi.IsChecked = false;

            ComboLLServices.IsEnabled = true;
            ComboLLServices.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            CheckboxSuivi.IsEnabled = true;
            CheckboxSuivi.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            CheckBoxStagiaire.IsEnabled = true;
            CheckBoxStagiaire.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            CheckBoxNonPhysique.IsEnabled = true;
            CheckBoxNonPhysique.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            CheckBoxAD.IsEnabled = true;
            CheckBoxAD.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            ComboADServices.IsEnabled = true;
            ComboADServices.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            CheckBoxMicro.IsEnabled = true;
            CheckBoxMicro.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            ComboMicroLicences.IsEnabled = true;
            ComboMicroLicences.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));


            CheckBoxFTP.IsEnabled = true;
            CheckBoxFTP.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        private void RadioRMTemplateClick(object sender, RoutedEventArgs e)
        {
            ComboLLServices.SelectedItem = "ADV/Commercial";
            CheckboxSuivi.IsChecked = true;

            ComboLLServices.IsEnabled = false;
            ComboLLServices.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            CheckboxSuivi.IsEnabled = false;
            CheckboxSuivi.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            CheckBoxStagiaire.IsEnabled = false;
            CheckBoxStagiaire.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            CheckBoxNonPhysique.IsEnabled = false;
            CheckBoxNonPhysique.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));


            CheckBoxAD.IsEnabled = false;
            CheckBoxAD.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            ComboADServices.IsEnabled = false;
            ComboADServices.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            CheckBoxMicro.IsEnabled = false;
            CheckBoxMicro.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            ComboMicroLicences.IsEnabled = false;
            ComboMicroLicences.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));

            CheckBoxFTP.IsEnabled = false;
            CheckBoxFTP.Foreground = new SolidColorBrush(Color.FromRgb(70, 74, 71));
        }

        private void SuiviRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            CheckBoxStagiaire.IsChecked = false;
        }

        private void StagiaireRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            CheckboxSuivi.IsChecked = false;
        }

        private void ButtonCreateUserClick(object sender, RoutedEventArgs e)
        {
            if (CheckBoxLL.IsChecked == true && ComboLLServices.SelectedIndex != -1 && ComboLLAgences.SelectedIndex != -1)
            {
                this.DialogResult = true;
            }
            else if ((CheckBoxMicro.IsChecked == true && ComboMicroLicences.SelectedItem.ToString() == "Licence Basic" && MicroRemainingAvailableBasicLicenses == 0) ||
                     (CheckBoxMicro.IsChecked == true && ComboMicroLicences.SelectedItem.ToString() == "Licence Standard" && MicroRemainingAvailableStandardLicenses == 0) )
            {
                Error.Content = "Veuillez libérer des licences basic/standard, il en manque";
            }
            else
            {
                Error.Content = "Veuillez remplir tous les champs, la création sur LeLog de l'utilisateur est obligatoire";
            }
        }
    }
}
