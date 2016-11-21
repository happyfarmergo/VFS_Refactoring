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
    /// Interaction logic for Writer.xaml
    /// </summary>
    public partial class Writer : Window
    {
        private File file;
        private Label label;
        private String labelFormat;
        private bool textChanged;
        public Writer()
        {
            InitializeComponent();
        }


        private void InitializeBindings()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help,
                (sender, e) =>
                {
                    (new AboutWindow()).Show();
                }, (sender, e) =>
                {
                    e.CanExecute = true;
                }));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save,
                (sender, e) =>
                {
                    saveFile();
                    textChanged = false;
                },
                (sender, e) =>
                {
                    e.CanExecute = true;
                }));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
                (sender, e) =>
                {
                    saveFile();
                    this.Close();
                },
                (sender, e) =>
                {
                    e.CanExecute = true;
                }));
        }

        public Writer(FileEntry entry, Label label, string format)
        {
            InitializeComponent();
            InitializeBindings();
            textChanged = false;

            this.file = (File)entry;
            this.label = label;
            this.labelFormat = format;
            string text = file.content;
            Dispatcher.Invoke(new Action(delegate
            {
                this.textbox.AppendText(text);
                textChanged = false;
            }));
        }

        private void saveFile()
        {
            file.content = new TextRange(textbox.Document.ContentStart,
                    textbox.Document.ContentEnd).Text;
            file.modifiedTime = Utility.getCurrentTime();
            MyFileSystem fs = MyFileSystem.Instance();
            fs.UpdateFile(file);
            label.Content = String.Format(labelFormat, fs.DiskSize, fs.SpaceUsed, fs.DiskSize - fs.SpaceUsed);
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textChanged = true;
        }

        private void IncreaseSize_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (textbox.FontSize > 40)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void IncreaseSize_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            textbox.FontSize += 4;
        }

        private void DecreaseSize_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (textbox.FontSize < 10)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void DecreaseSize_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            textbox.FontSize -= 4;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            if (!textChanged)
                return;
            MessageBoxResult result = MessageBox.Show(this,
                "Do you want to save changes to " + Title + "?",
                "Writer",
                MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                //save the changes
                saveFile();
            }
            else if (result == MessageBoxResult.No)
            {
                //don't save the changes
            }
            base.OnClosing(e);
        }
    }
}
