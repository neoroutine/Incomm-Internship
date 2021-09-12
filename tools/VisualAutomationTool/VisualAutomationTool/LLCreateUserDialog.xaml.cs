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
    /// Logique d'interaction pour LLCreateUserDialog.xaml
    /// </summary>
    public partial class LLCreateUserDialog : Window
    {
        public LLCreateUserDialog(List<string> agences, List<string> services)
        { 

            InitializeComponent();
            this.DataContext = this;

            this.Agences = agences;
            this.Services = services;
        }

        public List<string> Agences { get; set; }
        public List<string> Services { get; set; }

        public string EmailPasswordInformation { get; set; }

        private void ComboServicesSelectionChanged(object sender, RoutedEventArgs e)
        {
            //if services du lab alors ne pas afficher suivi
        }

        private void SuiviRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            CheckBoxStagiaire.IsChecked = false;
        }

        private void StagiaireRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            CheckboxSuivi.IsChecked = false;
        }

        private void CreateUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (ComboAgences.SelectedIndex != -1)
            {
                this.DialogResult = true;
            }
            
            else
            {
                Error.Content = "Veuillez remplir tous les champs";
            }
        }
    }
}
