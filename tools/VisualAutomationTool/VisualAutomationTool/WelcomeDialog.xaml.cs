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
    /// Logique d'interaction pour WelcomeDialog.xaml
    /// </summary>
    public partial class WelcomeDialog : Window
    {
        public WelcomeDialog(string username, bool secretFileSuccess)
        {
            InitializeComponent();
            this.DataContext = this;

            this.WelcomeUsername = username;
            this.FoundSecretFile = secretFileSuccess ? "Le fichier secret a été trouvé, hooray !" : "Le fichier secret n'a pas été trouvé, veuillez en fournir un dans le répertoire où se situe VAT";
        }

        public string WelcomeUsername { get; set; }

        public string FoundSecretFile { get; set; }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
