using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace VFS
{
    public enum EnumFileType : int
    {
        Folder = 0,
        TxtFile = 1
    }

    //public interface Iterator
    //{
    //    bool hasNext();
    //    object next();
    //}


    [Serializable]
    public class FileEntry : Object, ICloneable
    {
        public EnumFileType fileType;
        public string fileName;
        public string createdTime;
        public string modifiedTime;
        public int size;
        public int firstBlock;
        public int lastBlock;
        public FileEntry parent;

        public FileEntry()
        {

        }

        public FileEntry(string filename, string createdtime, string modifiedtime, int size, int firstblock, int lastblock, FileEntry entry = null)
        {
            this.fileName = filename;
            this.createdTime = createdtime;
            this.modifiedTime = modifiedtime;
            this.size = size;
            this.firstBlock = firstblock;
            this.lastBlock = lastblock;
            this.parent = entry;
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public string getFileLoc()
        {
            string result = "";
            FileEntry node = this;
            while (node != null)
            {
                result = "\\" + node.fileName + result;
                node = node.parent;
            }
            return "LJH:" + result;
        }

        public void SetName(string name)
        {
            this.fileName = name;
            this.modifiedTime = Utility.getCurrentTime();
        }

        public List<string> getFilePath()
        {
            List<string> dirArray = new List<string>();
            FileEntry node = this;
            while (node != null)
            {
                dirArray.Add(node.fileName);
                node = node.parent;
            }
            dirArray.Reverse();
            return dirArray;
        }

    }

    [Serializable]
    public class File : FileEntry
    {
        public string content;

        public File(FileEntry entry, string content = null)
        {
            this.fileName = entry.fileName;
            this.createdTime = entry.createdTime;
            this.modifiedTime = entry.modifiedTime;
            this.size = entry.size;
            this.firstBlock = entry.firstBlock;
            this.lastBlock = entry.lastBlock;
            this.parent = entry.parent;
            this.content = content;
            this.fileType = EnumFileType.TxtFile;
        }

        public override object Clone()
        {
            return new File(
                new FileEntry(
                    this.fileName,
                    this.createdTime,
                    this.modifiedTime,
                    this.size,
                    this.firstBlock,
                    this.lastBlock,
                    this.parent),
                    this.content);
        }

    }

    [Serializable]
    public class Folder : FileEntry
    {
        public List<FileEntry> children;

        public class FolderIterator
        {
            private int cur, max;
            private Folder folder;

            public bool hasNext()
            {
                return cur <= max;
            }
            public FolderIterator(Folder folder)
            {
                this.folder = folder;
                cur = 0;
                max = folder.children.Count - 1;
            }

            public FileEntry next()
            {
                return folder.children[cur++];
            }
        }

        public FolderIterator GetIterator()
        {
            return new FolderIterator(this);
        }

        public Folder(FileEntry entry)
        {
            this.fileName = entry.fileName;
            this.createdTime = entry.createdTime;
            this.modifiedTime = entry.modifiedTime;
            this.size = entry.size;
            this.firstBlock = entry.firstBlock;
            this.lastBlock = entry.lastBlock;
            this.parent = entry.parent;
            this.fileType = EnumFileType.Folder;
            this.children = new List<FileEntry>();
        }

        private Folder(FileEntry entry, List<FileEntry> children)
        {
            this.fileName = entry.fileName;
            this.createdTime = entry.createdTime;
            this.modifiedTime = entry.modifiedTime;
            this.size = entry.size;
            this.firstBlock = entry.firstBlock;
            this.lastBlock = entry.lastBlock;
            this.parent = entry.parent;
            this.fileType = EnumFileType.Folder;
            this.children = new List<FileEntry>();

            foreach (FileEntry file in children)
            {
                file.parent = this;
                this.children.Add((FileEntry)file.Clone());
            }

        }

        public override object Clone()
        {
            return new Folder(
                new FileEntry(
                    this.fileName,
                    this.createdTime,
                    this.modifiedTime,
                    this.size,
                    this.firstBlock,
                    this.lastBlock,
                    this.parent),
                    this.children);
        }

        public void InsertChild(FileEntry child)
        {
            this.children.Add(child);
            child.parent = this;
        }

        public FileEntry FindChild(string name)
        {
            foreach (FileEntry entry in this.children)
            {
                if (entry.fileName.Equals(name))
                {
                    return entry;
                }
            }
            return null;
        }

        public FileEntry removeChild(string name)
        {
            FileEntry target = FindChild(name);
            FileEntry copy = (FileEntry)target.Clone();
            this.children.Remove(target);
            return copy;
        }

        public void DeleteChild(FileEntry entry)
        {
            this.children.Remove(entry);
        }

        public void DeleteChild(string name)
        {
            this.children.Remove(this.FindChild(name));
        }

        public int FindSameName(string name)
        {
            foreach (FileEntry entry in this.children)
            {
                if (entry.fileName.Equals(name))
                    return 1;
            }
            return 0;
        }

        public bool IsChildOf(FileEntry root)
        {
            FileEntry entry = this;
            while (entry != null && entry != root)
            {
                entry = entry.parent;
            }
            return entry != null;
        }
    }
}
