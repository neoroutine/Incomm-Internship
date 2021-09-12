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
    /// Logique d'interaction pour LLDeleteUserDialog.xaml
    /// </summary>
    public partial class LLDeleteUserDialog : Window
    {
        public LLDeleteUserDialog(List<string> services, List<string> staffUsers)
        {
            InitializeComponent();
            this.DataContext = this;

            this.Services   = services;
            this.StaffUsers = staffUsers;
        }

        public List<string> Services { get; set; }
        public List<string> StaffUsers { get; set; }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            foreach (var user in StaffUsers)
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
