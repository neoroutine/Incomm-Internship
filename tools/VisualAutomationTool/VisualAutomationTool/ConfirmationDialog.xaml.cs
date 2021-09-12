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
    /// Logique d'interaction pour ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string question, string yes, string no, bool fullscreen = false)
        {
            InitializeComponent();
            this.DataContext = this;

            this.ConfirmationQuestion = question;
            this.ConfirmationYes      = yes;
            this.ConfirmationNo       = no;

            this.No.Focus();

            if (fullscreen) { this.WindowState = WindowState.Maximized; }
        }

        public string ConfirmationQuestion { get; set; }
        public string ConfirmationYes { get; set; }
        public string ConfirmationNo { get; set; }

        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
