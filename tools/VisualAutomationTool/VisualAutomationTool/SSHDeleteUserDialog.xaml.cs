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
    /// Logique d'interaction pour SSHDeleteUserDialog.xaml
    /// </summary>
    public partial class SSHDeleteUserDialog : Window
    {
        public SSHDeleteUserDialog(List<string> users)
        {
            InitializeComponent();
            this.DataContext = this;

            this.Users = users;
        }

        public List<string> Users { get; set; }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            foreach(var user in Users)
            {
                if (user.Contains(UserInput.Text))
                {
                    ComboUsers.SelectedItem = user;
                }
            }
        }

        private void ButtonDeleteUserClick(object sender, RoutedEventArgs e)
        {
            if (ComboUsers.SelectedIndex != -1)
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
