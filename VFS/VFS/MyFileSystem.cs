using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace VFS
{

    public enum UndoRedoCmdType : int
    {
        New = 0,
        Delete,
        CopyPaste,
        CutPaste,
        Rename
    }

    [Serializable]
    public class ShortFileInfo
    {
        public string name { get; set; }
        public EnumFileType type { get; set; }
        public BitmapImage image { get; set; }
        public string modifiedTime { get; set; }
        public string size { get; set; }            //kb
        public ShortFileInfo(string name, EnumFileType type, string modifiedtime, int size)
        {
            this.name = name;
            this.type = type;
            this.modifiedTime = modifiedtime;
            this.size = size / 1024 + "kb";
            string uri = (type == EnumFileType.TxtFile ? "images/file.png" : "images/folder.png");
            this.image = new BitmapImage(new Uri(uri, UriKind.Relative));
        }
    }


    [Serializable]
    public class FileOperation
    {
        public UndoRedoCmdType type;
        public FileEntry source;
        public FileEntry target;
        public string name;
        public EnumFileType fileType;
        public FileOperation(UndoRedoCmdType type, string name, FileEntry source, FileEntry target = null, EnumFileType fileType = EnumFileType.TxtFile)
        {
            this.type = type;
            this.source = source;
            this.target = target;
            this.name = name;
            this.fileType = fileType;
        }
    }


    [Serializable]
    public class ClipBoard
    {
        public CmdType type;
        public FileEntry Source { get; set; }        //the source folder
        public FileEntry Target { get; set; }        //the entry to paste

        public ClipBoard()
        {
            type = CmdType.Paste;                 //just for init
            Source = Target = null;
        }

        public void Clear()
        {
            type = CmdType.Paste;                 //just for init
            Source = Target = null;
        }

        public bool Empty()
        {
            return type == CmdType.Paste;
        }
    }

    [Serializable]
    class MyFileSystem
    {
        private static MyFileSystem instance;

        public static MyFileSystem Instance()
        {
            if (instance == null)
            {
                instance = new MyFileSystem();
            }
            return instance;
        }

        private MyDiskManager diskManager;
        private ClipBoard clipBoard;
        private Folder systemRoot;
        public Folder currentDir;

        public bool CanPaste
        {
            get
            {
                return !clipBoard.Empty();
            }
        }

        public bool CanUndo
        {
            get
            {
                return undoList.Count > 0;
            }
        }

        public bool CanRedo
        {
            get
            {
                return redoList.Count > 0;
            }
        }

        [NonSerialized]
        private List<ShortFileInfo> fileInfo;
        public List<ShortFileInfo> FileInfo
        {
            get
            {
                fillInfo();
                return fileInfo;
            }
        }

        [NonSerialized]
        private List<DirNode> dirTree;
        public List<DirNode> DirTree
        {
            get
            {
                getDirTree();
                return dirTree;
            }
        }

        private Stack<FileOperation> undoList;
        private Stack<FileOperation> redoList;

        public int SpaceUsed
        {
            get
            {
                return diskManager.Used;
            }
        }
        public int DiskSize = MyDiskManager.BLOCK_NUM;


        protected MyFileSystem()
        {
            diskManager = MyDiskManager.Instance();
            clipBoard = new ClipBoard();

            FileEntry node = null;
            systemRoot = (Folder)newFileNode("root", EnumFileType.Folder);
            diskManager.AllocFile(systemRoot);

            systemRoot.InsertChild(node = newFileNode("bin", EnumFileType.Folder));
            diskManager.AllocFile(node);

            systemRoot.InsertChild(node = newFileNode("user", EnumFileType.Folder));
            diskManager.AllocFile(node);

            currentDir = (Folder)node;
            currentDir.InsertChild(node = newFileNode("demo_txt", EnumFileType.TxtFile));
            diskManager.AllocFile(node);
            ((File)node).content = "My Dad worked hard but was a fun loving man. Remember when my dad worked 4 pm to midnight shift. I was 8 at the time. I put our dogs left over can food in a dish and into the fridge. In the morning at breakfast my mom asked what happened to the dog food Dave put in the fridge? My Dad said Dog food? I thought it was canned hash and ate it fried with my eggs. I was laughing and Dad said what's so funny it was very good." +
                " Then all 5 of our family members bought Dad canned dog food for his birthday as a joke. Was a funny and happy birthday for Dad. Now he has been gone many years but I will always remember this time and the Love he had for his kids and wife.\n" +
                "Another funny thing Dad did. We all were taking a local bus home from a movie night as a family. A lady said hello to my Dad and he said do I know you. She said yes I work as a nurse where you are the maintenance man. Oh he said (she was not in her nurses outfit), so he said I did not recognize you with clothes on. The look on our Mom's face made us laugh for a long time. Dad never understood what he said that was so funny.\n" +
                "When I turned 15 I was allowed to spend vacation time by my grandmother. She was very tidy, and I would not understand why she would keep this big aluminum ladle with all kind of bumps and dents on it. When I asked her why , she said to me, \"This ladle is your family's history. This dent here was made by your father\'s head, this one comes from your aunt C… this one here from your aunt J…. , like I said, it is your family\'s history, which is why I keep it\n" +
                "My father and his friends had gone fishing and brought back so much mullet fish that they could feed the whole village, which was rather unusual, mullet fish being hard to catch. Nazi soldiers had just been defeated and sent back as prisoners of war. They had destroyed as many weapons as they could, but some had been forgotten in hideouts. My father (a teenager at the time) and his friends had found a highly efficient and fast way to catch fish\n" +
                "They would attach a rope to the handle of hand grenades, trigger them, spin them around their head and throw them in the harbor where the explosion would bring whole schools of fish to the surface….\n" +
                "My father did just that, on this day and when he reached my grandmother's house; with a big smile on his face, he had a big surprise: Whahack…. The ladle crashed on his head……\nIt was at a time before internet and even before phone lines were installed, but at a time when any news about wrongdoing would travel at the speed of sound……\nHe learned the lesson the hard way…It was the last time he went fishing with hand grenades\n" +
                "To be honest, I secretly admit that I am not a good father, seldom do I take great interest in playing games or reading fairy tales to my children. Not once did I accompany my children to those extra classes which are aimed at developing childrens' interests. On the National holiday in 2003,my wife and I planned to take our daughter to visit the zoo. We did enjoy ourselves at first, watching all kinds of rare animals, feeding bananas to monkeys," +
                " fishing in a clear small pond. I was a bit tired, though, by my daughter's endless questions concerning the names and characteristic of the caged animals. But everything changed when we came to see the bears. There were fences all around, my daughter stood beside the fence, holding the metal rails. To see the bears more clearly, she mysteriously reached her head inside metal rails. The most frightening thing of all was that she could not get her head" +
                " out of the metal rails no matter how hard she tried. she cried and screamed , attracting almost all other visitors. Everyone was watching and offering advice, my daughter became the centre of attention. I rushed to the animal keepers for help. Two of them arrived on the scene with a ladder. They climbed over the fence, led the bears into the cave and finally saved my daughter. Tears in eyes and sweats all over, I thanked them for their timely help. " +
                "I was to blame for I failed to watch her closely. A conscientious father could never ignore her child only to amuse himself.";
            diskManager.UpdateFile(node, ((File)node).content.Length);
            currentDir = systemRoot;

            fileInfo = new List<ShortFileInfo>();
            undoList = new Stack<FileOperation>();
            redoList = new Stack<FileOperation>();
        }


        private FileEntry newFileNode(string name, EnumFileType type)
        {
            FileEntry entry;
            FileEntry inner = new FileEntry(
                        name,
                        Utility.getCurrentTime(),
                        Utility.getCurrentTime(),
                        0, 0, 0);

            if (type == EnumFileType.TxtFile)
                entry = new File(inner);
            else
                entry = new Folder(inner);
            return entry;
        }

        private FileEntry copyFileNode(FileEntry from, FileEntry parent)
        {
            if (from == null)
                return null;
            FileEntry to = newFileNode(from.fileName, from.fileType);
            if (to.fileType == EnumFileType.TxtFile)
            {
                ((File)to).content = (string)((File)from).content.Clone();
                to.size = from.size;
                diskManager.AllocFile(to);
            }
            else
            {
                Folder source = (Folder)from;
                for (int i = 0; i < source.children.Count; ++i)
                {
                    source.children[i] = copyFileNode(source.children[i], source);
                }
            }
            to.parent = parent;
            return to;
        }

        private void freeFileNode(FileEntry from)
        {
            if (from.fileType == EnumFileType.TxtFile)
            {
                diskManager.FreeFile(from);
            }
            else
            {
                Folder source = (Folder)from;
                Folder.FolderIterator iterator = source.GetIterator();
                while (iterator.hasNext())
                {
                    freeFileNode(iterator.next());
                }
            }
        }

        private void fillInfo()
        {
            fileInfo = new List<ShortFileInfo>();
            foreach (FileEntry node in currentDir.children)
            {
                fileInfo.Add(new ShortFileInfo(node.fileName, node.fileType, node.modifiedTime, node.size));
            }
        }

        //?
        private string getRightName(string name, FileEntry dir = null)
        {
            if (dir == null) dir = currentDir;
            string save = name;
            int cnt = 0, num = 1;
            while ((cnt = ((Folder)dir).FindSameName(name)) > 0)
            {
                name = save + "_" + num++;
            }
            return name;
        }

        public string NewFile(string name, EnumFileType type)
        {
            name = getRightName(name);
            FileEntry node = newFileNode(name, type);
            diskManager.AllocFile(node);
            currentDir.InsertChild(node);
            undoList.Push(new FileOperation(UndoRedoCmdType.New, name, currentDir, null, type));
            return name;
        }

        public bool RenameFile(string oldName, string newName)
        {
            FileEntry node = currentDir.FindChild(oldName);

            Debug.Assert(node != null);

            int cnt = currentDir.FindSameName(newName);
            if (cnt > 0) return false;

            node.SetName(newName);
            undoList.Push(new FileOperation(UndoRedoCmdType.Rename, oldName, node));
            return true;
        }

        public void CutFile(string name)
        {
            clipBoard.Clear();
            clipBoard.type = CmdType.Cut;
            clipBoard.Source = currentDir.FindChild(name);
        }

        public void CopyFile(string name)
        {
            clipBoard.Clear();
            clipBoard.type = CmdType.Copy;
            clipBoard.Source = currentDir.FindChild(name);
        }

        public void DeleteFile(string name)
        {
            FileEntry entry = currentDir.removeChild(name);
            diskManager.DuplicateFreeFile(entry);
            undoList.Push(new FileOperation(UndoRedoCmdType.Delete, name, entry, currentDir));
        }


        public bool PasteFile()
        {
            if (currentDir.IsChildOf(clipBoard.Source)) return false;
            string newName = null;
            switch (clipBoard.type)
            {
                case CmdType.Cut:
                    FileEntry tocut = ((Folder)clipBoard.Source.parent).removeChild(clipBoard.Source.fileName);
                    newName = clipBoard.Source.fileName = getRightName(clipBoard.Source.fileName, clipBoard.Source.parent);
                    undoList.Push(new FileOperation(UndoRedoCmdType.CutPaste, newName, clipBoard.Source.parent, currentDir));
                    currentDir.InsertChild(tocut);
                    break;
                case CmdType.Copy:
                    FileEntry tocopy = (FileEntry)clipBoard.Source.Clone();
                    newName = tocopy.fileName = getRightName(tocopy.fileName, currentDir);
                    undoList.Push(new FileOperation(UndoRedoCmdType.CopyPaste, newName, clipBoard.Source.parent, currentDir));
                    currentDir.InsertChild(tocopy);
                    break;
                case CmdType.Paste:
                    break;

            }
            clipBoard.Clear();
            return true;
        }

        public void UndoCmd()
        {
            if (undoList.Count > 0)
            {
                FileOperation op = undoList.Peek();

                switch (op.type)
                {
                    case UndoRedoCmdType.New:
                        Folder source = op.source as Folder;
                        FileEntry target = source.FindChild(op.name);
                        diskManager.FreeFile(target);
                        source.DeleteChild(target);
                        undoList.Pop();
                        redoList.Push(op);
                        break;
                    case UndoRedoCmdType.Delete:
                        ((Folder)op.target).InsertChild(op.source);
                        diskManager.DuplicateAllocFile(op.source);
                        undoList.Pop();
                        redoList.Push(op);
                        break;
                    case UndoRedoCmdType.Rename:
                        string newName = op.source.fileName;
                        op.source.SetName(op.name);
                        undoList.Pop();
                        op.name = newName;
                        redoList.Push(op);
                        break;
                    case UndoRedoCmdType.CutPaste:
                        FileEntry file = ((Folder)op.target).removeChild(op.name);
                        ((Folder)op.source).InsertChild(file);
                        undoList.Pop();
                        redoList.Push(op);
                        break;
                    case UndoRedoCmdType.CopyPaste:
                        ((Folder)op.target).DeleteChild(op.name);
                        undoList.Pop();
                        redoList.Push(op);
                        break;
                }
            }
        }

        public void RedoCmd()
        {
            if (redoList.Count > 0)
            {
                FileOperation op = redoList.Peek();
                FileEntry source, target, entry, file;
                source = target = entry = file = null;
                switch (op.type)
                {
                    case UndoRedoCmdType.New:
                        target = newFileNode(op.name, op.fileType);
                        ((Folder)op.source).InsertChild(target);
                        diskManager.AllocFile(target);
                        redoList.Pop();
                        undoList.Push(op);
                        break;
                    case UndoRedoCmdType.Delete:
                        entry = ((Folder)op.target).removeChild(op.source.fileName);
                        diskManager.DuplicateFreeFile(entry);
                        redoList.Pop();
                        undoList.Push(op);
                        break;
                    case UndoRedoCmdType.Rename:
                        string oldName = op.source.fileName;
                        op.source.SetName(op.name);
                        redoList.Pop();
                        op.name = oldName;
                        undoList.Push(op);
                        break;
                    case UndoRedoCmdType.CutPaste:
                        file = ((Folder)op.source).removeChild(op.name);
                        ((Folder)op.target).InsertChild(file);
                        redoList.Pop();
                        undoList.Push(op);
                        break;
                    case UndoRedoCmdType.CopyPaste:
                        entry = (FileEntry)((Folder)op.source).FindChild(op.name).Clone();
                        ((Folder)op.target).InsertChild(entry);
                        redoList.Pop();
                        undoList.Push(op);
                        break;
                }
            }
        }

        

        public void EnterNextDir(string name)
        {
            currentDir = (Folder)currentDir.FindChild(name);
        }

        public bool ReturnPreDir()
        {
            if (currentDir == systemRoot) return false;
            currentDir = (Folder)currentDir.parent;
            return true;
        }

        public string GetContent(string name)
        {
            return ((File)currentDir.FindChild(name)).content;
        }

        public void UpdateFile(FileEntry node)
        {
            diskManager.UpdateFile(node, ((File)node).content.Length);
        }

        public int GetFileSizeOnDisk(FileEntry header)
        {
            return diskManager.GetFileSizeOnDisk(header);
        }

        public bool ChangeDir(List<string> path)
        {
            FileEntry node = systemRoot;
            for (int i = 1; i < path.Count; ++i)
            {
                node = ((Folder)node).FindChild(path[i]);
                if (node == null) return false;
            }

            currentDir = (Folder)node;
            return true;
        }

        private void getDirTree()
        {
            List<DirNode> root = new List<DirNode>();
            root.Add(new DirNode(systemRoot.fileName));
            bindDir(root[0], systemRoot.children);
            dirTree = root;
        }

        private void bindDir(DirNode node, List<FileEntry> dir)
        {
            foreach (FileEntry entry in dir)
            {
                DirNode tmp = null;
                if (entry.fileType == EnumFileType.Folder)
                {
                    tmp = new DirNode(entry.fileName, node);
                    node.Nodes.Add(tmp);
                    bindDir(tmp, ((Folder)entry).children);
                }
            }
        }
    }
}
