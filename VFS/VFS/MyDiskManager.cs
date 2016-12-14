using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace VFS
{
    [Serializable]
    class MyDiskManager
    {
        private static MyDiskManager instance;
        public static MyDiskManager Instance()
        {
            if (instance == null)
                instance = new MyDiskManager();
            return instance;
        }

        public static void SetInstance(MyDiskManager dm)
        {
            instance = dm;
        }

        //64MB
        public static int BLOCK_SIZE = 1024;
        public static int BLOCK_NUM = 1024 * 64;
        public static int DISK_SIZE = BLOCK_SIZE * BLOCK_NUM;

        private MyDisk disk;
        private byte[] bitmap;
        private int mapHead;                              //first free block
        public int Used { get; set; }

        protected MyDiskManager()
        {
            disk = new MyDisk(BLOCK_SIZE, BLOCK_NUM);
            bitmap = new byte[BLOCK_NUM + 1];
            for (int i = 1; i <= BLOCK_NUM; ++i) bitmap[i] = 0;
            mapHead = 1;
            Used = 0;
        }

        public void AllocFile(FileEntry fcb)
        {
            int blockNum = numOfBlock(fcb.size);
            if (fcb.fileType == EnumFileType.Folder || blockNum == 0) return;

            Used += blockNum;

            int end = allocFrom(mapHead, blockNum);
            fcb.firstBlock = mapHead;
            fcb.lastBlock = end;
            mapHead = nextFreeBlock(end + 1);
        }

        public void FreeFile(FileEntry fcb)
        {
            int blockNum = numOfBlock(fcb.size);
            if (fcb.fileType == EnumFileType.Folder || blockNum == 0) return;

            Used -= blockNum;

            freeFrom(fcb.firstBlock, fcb.lastBlock);
            if (fcb.firstBlock < mapHead) mapHead = fcb.firstBlock;
        }

        public void DuplicateAllocFile(FileEntry fcb)
        {
            if (fcb.fileType == EnumFileType.TxtFile)
            {
                AllocFile(fcb);
            }
            else
            {
                foreach (FileEntry entry in ((Folder)fcb).children)
                {
                    DuplicateAllocFile(entry);
                }
            }
        }

        public void DuplicateFreeFile(FileEntry fcb)
        {
            if (fcb.fileType == EnumFileType.TxtFile)
            {
                FreeFile(fcb);
            }
            else
            {
                foreach (FileEntry entry in ((Folder)fcb).children)
                {
                    DuplicateFreeFile(entry);
                }
            }
        }

        public void UpdateFile(FileEntry fcb, int targetSize)
        {
            FreeFile(fcb);
            fcb.size = targetSize;
            AllocFile(fcb);
        }

        public int numOfBlock(int size)
        {
            int blockNum = size / BLOCK_SIZE;
            if (size % BLOCK_SIZE != 0) blockNum += 1;
            return blockNum;
        }

        public int GetFileSizeOnDisk(FileEntry fcb)
        {
            int start = fcb.firstBlock, blockNum = 0;
            if (start == 0) return 0;
            while (start != -1)
            {
                start = disk.diskSpace[start];
                blockNum++;
            }
            return blockNum;
        }

        private int nextFreeBlock(int pre)
        {
            while (pre <= BLOCK_NUM && bitmap[pre] == 1)
                ++pre;
            if (pre <= BLOCK_NUM) return pre;
            return -1;
        }

        private int allocFrom(int start, int length)             //the last of allocated
        {

            for (int i = start, pre = 0; i <= BLOCK_NUM; ++i)
            {
                if (bitmap[i] == 1) continue;

                if (pre != 0)
                    disk.diskSpace[pre] = i;

                length--;
                bitmap[i] = 1;
                if (length <= 0)
                {
                    disk.diskSpace[i] = -1;
                    return i;
                }

                pre = i;
            }

            return -1;
        }

        private void freeFrom(int start, int end)
        {
            while (start != end)
            {
                bitmap[start] = 0;
                start = disk.diskSpace[start];
            }
            Debug.Assert(disk.diskSpace[start] == -1);
            bitmap[start] = 0;
        }
    }
}
