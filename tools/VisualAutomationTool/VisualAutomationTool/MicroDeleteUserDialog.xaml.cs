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
    /// Logique d'interaction pour MicroDeleteUserDialog.xaml
    /// </summary>
    public partial class MicroDeleteUserDialog : Window
    {
        public MicroDeleteUserDialog(MicrosoftManager microManager, List<string> users)
        {
            InitializeComponent();
            this.DataContext = this;

            this.MicroManager = microManager;
            this.Users = users;

            this.UserInput.Focus();
        }

        public MicrosoftManager MicroManager { get; set; }

        public List<string> Users { get; set; }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {

            foreach (var user in this.Users)
            {
                if (user.Contains(this.UserInput.Text))
                {
                    this.UsersCombo.SelectedItem = user;
                }
            }

        }

        private void DeleteUserButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.UsersCombo.SelectedIndex != -1)
            {
                this.DialogResult = true;
            }
            else
            {
                this.Error.Content = "Veuillez sélectionner un utilisateur à supprimer";
            }
        }
    }
}
