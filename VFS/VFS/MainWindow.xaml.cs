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
            UpdateShowGrid(mFs.FileInfo);
        }



        MyFileSystem mFs;
        List<DirNode> mDirTree;
        List<string> mOpenedWriter;
        List<string> mOpenedRename;
        List<List<string>> mHistoryPath;
        List<List<string>> mFuturePath;

        const string saveLoc = "MyFileSystem.dat";
        const string saveLoc2 = "TreeView.dat";

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

            if (!Deserialize())
            {
                List<DirNode> dirViewNodes = new List<DirNode>(){
                new DirNode{ID = 1, Name = "root"},
                new DirNode{ID = 2, Name = "user", ParentID = 1},
                new DirNode{ID = 3, Name = "bin", ParentID = 1},
                };

                mDirTree = DirNode.BindDir(dirViewNodes);
                mFs = MyFileSystem.Instance();
                mFs.EnterNextDir("user");
                mHistoryPath.Add(mFs.currentDir.getFilePath());
            }
            this.dirView.ItemsSource = mDirTree;
            tbDir.Text = mFs.currentDir.getFileLoc();
            UpdateShowGrid(mFs.FileInfo);
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

            List<string> path = DirNode.FindPathByID(mDirTree[0], item.ID);
            mHistoryPath.Add(mFs.currentDir.getFilePath());
            if (mHistoryPath.Count > 20) mHistoryPath.RemoveAt(0);
            mFs.ChangeDir(path);
            tbDir.Text = mFs.currentDir.getFileLoc();
            UpdateShowGrid(mFs.FileInfo);
            e.Handled = true;
        }

        public void UpdateShowGrid(List<ShortFileInfo> fileInfo)
        {

        }

        public void UpdateTreeView()
        {

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
                UpdateShowGrid(mFs.FileInfo);
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
                UpdateShowGrid(mFs.FileInfo);
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
                UpdateShowGrid(mFs.FileInfo);
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

            mOpenedWriter = new List<string>();
            mOpenedRename = new List<string>();
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

                fstream = new FileStream(saveLoc2, FileMode.Open, FileAccess.Read);
                BinaryFormatter binFormat2 = new BinaryFormatter();
                mDirTree = (List<DirNode>)binFormat2.Deserialize(fstream);
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
            fstream = new FileStream(saveLoc2, FileMode.Create, FileAccess.ReadWrite);
            BinaryFormatter binFormatter2 = new BinaryFormatter();
            binFormatter2.Serialize(fstream, mDirTree);
            fstream.Close();
        }

    }
}
