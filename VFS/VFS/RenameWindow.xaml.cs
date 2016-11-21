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

namespace VFS
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        private string oldName;
        public RenameWindow()
        {
            InitializeComponent();
        }

        public RenameWindow(string name)
        {
            InitializeComponent();
            oldName = name;
            this.textbox.Text = name;
            this.textbox.SelectAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            string newName = textbox.Text;
            if (!string.Equals(oldName, newName) &&
                !MyFileSystem.Instance().RenameFile(oldName, newName))
            {
                MessageBox.Show(this,
                    "there is already a \"" + newName + "\" file in the current dir",
                    "warning",
                    MessageBoxButton.OK);
                return;
            }
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
