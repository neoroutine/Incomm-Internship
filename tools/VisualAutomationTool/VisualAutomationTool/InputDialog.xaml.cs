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
    /// Logique d'interaction pour InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string title)
        {
            InitializeComponent();
            this.DataContext = this;

            this.InputQuestion = title;

            this.Answer.Focus();
        }

        public string InputQuestion { get; set; }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Answer.Text.Length > 0)
            {
                this.DialogResult = true;
            }
            
            else
            {
                this.Error.Content = "Veuillez entrer une réponse";
            }
        }
    }
}
