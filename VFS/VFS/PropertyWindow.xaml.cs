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
    /// Interaction logic for PropertyWindow.xaml
    /// </summary>
    public partial class PropertyWindow : Window
    {
        public PropertyWindow()
        {
            InitializeComponent();
        }

        public PropertyWindow(FileEntry file)
        {
            InitializeComponent();
            string uri = (file.fileType == EnumFileType.TxtFile ?
               "images/file.png" : "images/folder.png");
            this.image.Source = new BitmapImage(new Uri(uri, UriKind.Relative));
            this.textbox.Text = file.fileName;
            this.tbFileType.Text = (file.fileType == EnumFileType.TxtFile ? "Txt" : "Folder");
            this.tbFileLoc.Text = file.getFileLoc();
            this.tbFileSize.Text = file.size.ToString() + " bytes";
            this.tbFileSizeOnDisk.Text = MyFileSystem.Instance().GetFileSizeOnDisk(file).ToString();
            this.tbFileCreated.Text = file.createdTime;
            this.tbFileModified.Text = file.modifiedTime;
            this.tbFileOnDiskLoc.Text = "start at " + file.firstBlock + ", end at " + file.lastBlock;
        }
    }
}
