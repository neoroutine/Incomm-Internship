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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace VisualAutomationTool.UserControls
{
    public partial class ContentDisplay : UserControl
    {
        public ContentDisplay(string title, string content, TextBox notepadEditor)
        {
            InitializeComponent();
            this.DataContext = this;

            this.Title                  = title;
            this.ContentDisplayed       = content;
            this.DedicatedNotepadEditor = notepadEditor;

            if (GetLongestLineLength(content) > 100) { this.ContentBody.Width = 100 * 5; }
            else if (GetLongestLineLength(content) < 50) { this.ContentBody.Width = 50 * 5; }
            else { this.ContentBody.Width = GetLongestLineLength(content) * 5; }

            if (GetNumberOfLines(content) > 15) { this.ContentBody.Height = 300; } 
            else if (GetNumberOfLines(content) < 5) { this.ContentBody.Height = 80; }
            else { this.ContentBody.Height = GetNumberOfLines(content) * 20; }
        }

        public string Title { get; set; }

        public string ContentDisplayed { get; set; }

        public TextBox DedicatedNotepadEditor { get; set; }

        private int GetNumberOfLines(string content)
        {
            int lines = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n') { lines++; }
            }

            return lines;
        }
        private int GetLongestLineLength(string content)
        {
            int lineLength = 0;
            int longestLineLength = 0;
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n')
                {
                    if (lineLength > longestLineLength) { longestLineLength = lineLength; }
                    lineLength = 0;
                }
                else
                {
                    lineLength++;
                }
            }

            if (longestLineLength > 140)
            {
                return 140;
            }
            return longestLineLength;
        }

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ContentBody.Text.ToString());
        }

        private void CopyToNotepadEditor(object sender, RoutedEventArgs e)
        {
            DedicatedNotepadEditor.Text = ContentBody.Text;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "All files(*.*) | *.*";
            if (saveFileDialog.ShowDialog() == true)
                System.IO.File.WriteAllText(saveFileDialog.FileName, ContentBody.Text.ToString());
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            ((StackPanel)this.Parent).Children.Remove(this);
        }
    }
}
