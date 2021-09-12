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
    /// Logique d'interaction pour SSHCreateUserDialog.xaml
    /// </summary>
    public partial class SSHCreateUserDialog : Window
    {
        public SSHCreateUserDialog()
        {
            InitializeComponent();
        }

        private void ButtonCreateUserClick(object sender, RoutedEventArgs e)
        {
            if (this.Prenom.Text.Length > 0 && this.Nom.Text.Length > 0)
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
