using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace VFS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public enum CmdType : int
    {
        New = 0,
        Delete,
        Cut,
        Copy,
        Paste,
        Rename,
        Undo
    }

    public partial class MainWindow : Window
    {

        private static RoutedCommand renameCmd;
        public static RoutedCommand RenameCmd { get { return renameCmd; } }
        public String LabelFormat = "Total Size: {0}kb  Used: {1}kb  Remain: {2}kb";

        static MainWindow()
        {
            InputGestureCollection gesture = new InputGestureCollection();
            gesture.Add(new KeyGesture(Key.M, ModifierKeys.Control, "Ctrl+M"));
            renameCmd = new RoutedCommand("Rename", typeof(MainWindow), gesture);

        }

        private void NewTxtFile_Executed(object sender, RoutedEventArgs e)
        {
            mFs.NewFile("new_text", EnumFileType.TxtFile);
            UpdateShowGrid();
        }


        private void NewFolder_Executed(object sender, RoutedEventArgs e)
        {
            string realName = mFs.NewFile("new_folder", EnumFileType.Folder);
            UpdateTreeView();
            UpdateShowGrid();
        }


        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenItem(GetSelectedEntry(this.dataGrid.SelectedItem));
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.dataGrid.SelectedItem != null;
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenItem(GetSelectedEntry(this.dataGrid.SelectedItem));
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileEntry entry = GetSelectedEntry(this.dataGrid.SelectedItem);
            mFs.DeleteFile(entry.fileName);
            if (entry.fileType == EnumFileType.Folder)
            {
                UpdateTreeView();
            }
            UpdateShowGrid();
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.dataGrid.SelectedItem != null;

        }


        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileEntry entry = GetSelectedEntry(this.dataGrid.SelectedItem);
            mFs.CutFile(entry.fileName);
            if (entry.fileType == EnumFileType.Folder)
            {
                UpdateTreeView();
            }
            UpdateShowGrid();
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.dataGrid.SelectedItem != null;

        }


        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FileEntry entry = GetSelectedEntry(this.dataGrid.SelectedItem);
            mFs.CopyFile(entry.fileName);
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.dataGrid.SelectedItem != null;
        }


        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mFs.PasteFile();
            UpdateTreeView();
            UpdateShowGrid();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mFs.CanPaste;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mFs.UndoCmd();
            UpdateTreeView();
            UpdateShowGrid();
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mFs.CanUndo;
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mFs.RedoCmd();
            UpdateTreeView();
            UpdateShowGrid();
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mFs.CanRedo;
        }

        private FileEntry GetSelectedEntry(object item)
        {
            ShortFileInfo fileInfo = (ShortFileInfo)item;
            Debug.Assert(fileInfo != null);
            return mFs.currentDir.FindChild(fileInfo.name);
        }

        private void Rename_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RenameWindow renameWindow = new RenameWindow(GetSelectedEntry(this.dataGrid.SelectedItem).fileName);
            renameWindow.Owner = this;
            renameWindow.Show();
        }

        private void Rename_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.dataGrid.SelectedItem != null;
        }


        private void Property_Executed()
        {
            FileEntry file = mFs.currentDir.FindChild(
                ((ShortFileInfo)this.dataGrid.SelectedItem).name);
            PropertyWindow window = new PropertyWindow(file);
            window.Owner = this;
            window.Show();
        }

        private MyFileSystem mFs;
        private List<List<string>> mHistoryPath;
        private List<List<string>> mFuturePath;

        private const string saveLoc = "MyFileSystem.dat";

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

            if (!Deserialize())
            {
                mFs = MyFileSystem.Instance();
                mFs.EnterNextDir("user");
            }
            this.dirView.ItemsSource = mFs.DirTree;
            this.dataGrid.ItemsSource = mFs.FileInfo;
            this.tbDir.Text = mFs.currentDir.getFileLoc();
        }

        private void OpenItem(FileEntry entry)
        {
            if (entry == null) return;
            if (entry.fileType == EnumFileType.Folder)
            {
                mFs.EnterNextDir(entry.fileName);
                mHistoryPath.Add(mFs.currentDir.getFilePath());
                if (mHistoryPath.Count > 20) mHistoryPath.RemoveAt(0);

                tbDir.Text = mFs.currentDir.getFileLoc();
                UpdateShowGrid();
            }
            else
            {
                Writer writer = new Writer(entry, rightLabel, LabelFormat);
                writer.Owner = this;
                writer.Title = entry.fileName;
                writer.Show();
            }
        }

        private void dirView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DirNode item = dirView.SelectedItem as DirNode;
            if (item == null)
            {
                e.Handled = true;
                return;
            }
            if (string.Equals(mFs.currentDir.fileName, item.Name))
                return;

            List<string> path = DirNode.FindPath(item);
            mHistoryPath.Add(mFs.currentDir.getFilePath());
            if (mHistoryPath.Count > 20) mHistoryPath.RemoveAt(0);
            mFs.ChangeDir(path);
            tbDir.Text = mFs.currentDir.getFileLoc();
            UpdateShowGrid();
            e.Handled = true;
        }

        public void UpdateShowGrid()
        {
            this.dataGrid.ItemsSource = null;
            this.dataGrid.ItemsSource = mFs.FileInfo;

            this.leftLabel.Content = "" + mFs.FileInfo.Count + " items";
            this.rightLabel.Content = String.Format(LabelFormat,
                mFs.DiskSize,
                mFs.SpaceUsed,
                mFs.DiskSize - mFs.SpaceUsed);
        }

        public void UpdateTreeView()
        {
            this.dirView.ItemsSource = null;
            this.dirView.ItemsSource = mFs.DirTree;
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            if (mHistoryPath.Count == 0) return;
            if (mFs.ChangeDir(mHistoryPath.Last()))
            {
                mFuturePath.Add(mHistoryPath.Last());
                if (mFuturePath.Count > 20) mFuturePath.RemoveAt(0);
                mHistoryPath.Remove(mHistoryPath.Last());
                tbDir.Text = mFs.currentDir.getFileLoc();
                UpdateShowGrid();
            }
            else
            {
                LocationNotAvailable(mHistoryPath.Last());
            }
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            if (mFuturePath.Count == 0) return;
            if (mFs.ChangeDir(mFuturePath.Last()))
            {
                mHistoryPath.Add(mFuturePath.Last());
                if (mHistoryPath.Count > 20) mHistoryPath.RemoveAt(0);
                mFuturePath.Remove(mFuturePath.Last());
                tbDir.Text = mFs.currentDir.getFileLoc();
                UpdateShowGrid();
            }
            else
            {
                LocationNotAvailable(mFuturePath.Last());
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if (mFs.ReturnPreDir())
            {
                mHistoryPath.Add(mFs.currentDir.getFilePath());
                if (mHistoryPath.Count > 20) mHistoryPath.RemoveAt(0);
                tbDir.Text = mFs.currentDir.getFileLoc();
                UpdateShowGrid();
            }
        }

        private void LocationNotAvailable(List<string> path)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LJH:");
            foreach (string s in path)
            {
                sb.Append("\\" + s);
            }
            sb.Append(" is unavailable.\nplease check your folders");
            MessageBox.Show(this, sb.ToString(), "Location is not available", MessageBoxButton.OK);
            mHistoryPath.Clear();
            mFuturePath.Clear();
        }


        private void Initialize()
        {
            mHistoryPath = new List<List<string>>();
            mFuturePath = new List<List<string>>();

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties,
                (sender, e) =>
                {
                    Property_Executed();
                }, (sender, e) =>
                {
                    e.CanExecute = true;
                }));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help,
                (sender, e) =>
                {
                    (new AboutWindow()).Show();
                }, (sender, e) =>
                {
                    e.CanExecute = true;
                }));
        }

        private bool Deserialize()
        {
            bool res = true;
            try
            {
                Stream fstream = new FileStream(saveLoc, FileMode.Open, FileAccess.Read);
                BinaryFormatter binFormat = new BinaryFormatter();
                mFs = (MyFileSystem)binFormat.Deserialize(fstream);
                fstream.Close();
            }
            catch (FileNotFoundException e)
            {
                res = false;
            }
            return res;
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Stream fstream = new FileStream(saveLoc, FileMode.Create, FileAccess.ReadWrite);
            BinaryFormatter binFormat = new BinaryFormatter();
            binFormat.Serialize(fstream, mFs);
            fstream.Close();
        }

    }
}
