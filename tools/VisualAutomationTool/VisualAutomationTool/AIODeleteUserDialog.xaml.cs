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
    /// Logique d'interaction pour AIODeleteUserDialog.xaml
    /// </summary>
    public partial class AIODeleteUserDialog : Window
    {
        public AIODeleteUserDialog(List<string> llUsers, List<string> adUsers, List<string> microUsers, List<string> ftpUsers)
        {
            InitializeComponent();
            this.DataContext = this;

            this.LLUsers = llUsers;
            this.ADUsers = adUsers;
            this.MicroUsers = microUsers;
            this.FTPUsers = ftpUsers;

            this.UserInput.Focus();
        }

        public List<string> LLUsers { get; set; }

        public List<string> ADUsers { get; set; }

        public List<string> MicroUsers { get; set; }

        public List<string> FTPUsers { get; set; }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (UserInput.Text.Length > 2)
            {
                var prenomNom = UserInput.Text.Split(" ");
                if (prenomNom.Length < 2) { prenomNom = new string[]{ prenomNom[0], ""}; }
                foreach (var user in LLUsers)
                {
                    if (user.ToLower().Contains(prenomNom[0].ToLower()) && user.ToLower().Contains(prenomNom[1].ToLower()))
                    {
                        ComboLLUsers.SelectedItem = user;
                    }
                }
            }
            
        }

        private void ComboLLUsersSelectionChanged(object sender, RoutedEventArgs e)
        {
            string userInfos = ComboLLUsers.SelectedItem.ToString();
            string userFullName = userInfos.Split("|")[0].Replace("_", " ").Trim();
            var prenomNom = userInfos.Split("|")[0].Trim().Split("_");
            string username = "";
            if (prenomNom[0].Length == 0 && prenomNom[1].Length == 0)
            {
                username = "";
            }
            else if (prenomNom[0].Length == 0)
            {
                username = prenomNom[1].ToLower();
            }
            else if (prenomNom[1].Length == 0)
            {
                username = prenomNom[0][0].ToString().ToLower();
            }
            else
            {
                username = prenomNom[0][0].ToString().ToLower() + prenomNom[1].ToLower();
            }

            //AD
            foreach(var user in ADUsers)
            {
                if (user.ToLower().Contains(prenomNom[0].ToLower()) && user.ToLower().Contains(prenomNom[1].ToLower()))
                {
                    ComboADUsers.SelectedItem = user;
                    break;
                }
            }
            
            //Micro
            foreach (var user in MicroUsers)
            {
                if (user.ToLower().Contains(prenomNom[0].ToLower()) && user.ToLower().Contains(prenomNom[1].ToLower()))
                {
                    ComboMicroUsers.SelectedItem = user;
                    break;
                }
            }
            
            //FTP
            foreach (var user in FTPUsers)
            {
                if (user.ToLower().Contains(username))
                {
                    ComboFTPUsers.SelectedItem = user;
                    break;
                }
            }
        }

        private void ButtonCreateUserClick(object sender, RoutedEventArgs e)
        {
            if (ComboLLUsers.SelectedIndex != -1)
            {
                bool valid = true;
                if (CheckBoxAD.IsChecked == true)
                { if (ComboADUsers.SelectedIndex == -1) { valid = false; } }

                if (CheckBoxMicro.IsChecked == true)
                { if (ComboMicroUsers.SelectedIndex == -1) { valid = false; } }

                if (CheckBoxFTP.IsChecked == true)
                { if (ComboFTPUsers.SelectedIndex == -1) { valid = false; } }

                if (valid)
                {
                    this.DialogResult = true;
                }
                else
                {
                    Error.Content = "Veuillez remplir tous les champs";
                }
            }
            else
            {
                Error.Content = "Veuillez remplir tous les champs";
            }
        }
    }
}
