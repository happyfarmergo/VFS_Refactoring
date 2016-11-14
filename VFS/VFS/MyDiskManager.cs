using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS
{
    class MyDiskManager
    {
        private static MyDiskManager instance;


        public static MyDiskManager Instance()
        {
            if (instance == null)
                instance = new MyDiskManager();
            return instance;
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
            mapHead = 1;
            Used = 0;
        }

        public void AllocFile(FileEntry fcb)
        {
            int blockNum = numOfBlock(fcb.size);
            Used += blockNum;

            int end = allocFrom(mapHead, blockNum);
            fcb.firstBlock = mapHead;
            fcb.lastBlock = end;
            mapHead = nextFreeBlock(end + 1);
        }

        public void FreeFile(FileEntry fcb)
        {
            int blockNum = numOfBlock(fcb.size);
            Used -= blockNum;

            freeFrom(fcb.firstBlock, blockNum);
            if (fcb.firstBlock < mapHead) mapHead = fcb.firstBlock;
        }

        public void UpdateFile(FileEntry fcb, int targetSize)
        {
            int fromNum = numOfBlock(fcb.size);
            int toNum = numOfBlock(targetSize);
            fcb.size = targetSize;
            if (toNum == fromNum) return;
            Used += toNum - fromNum;
            int extra;
            if (toNum > fromNum)
            {
                extra = toNum - fromNum;
                disk.diskSpace[fcb.lastBlock] = mapHead;
                int end = allocFrom(mapHead, extra);
                fcb.lastBlock = end;
                mapHead = nextFreeBlock(end + 1);
            }
            else
            {
                extra = fromNum - toNum;
                if (fcb.firstBlock < mapHead) mapHead = fcb.firstBlock;
                fcb.firstBlock = freeFrom(fcb.firstBlock, extra);
            }
        }

        public int numOfBlock(int size)
        {
            int blockNum = size / BLOCK_SIZE;
            if (size % BLOCK_SIZE != 0 || size == 0) blockNum += 1;
            return blockNum;
        }

        public int GetFileSizeOnDisk(FileEntry fcb)
        {
            int start = fcb.firstBlock, blockNum = 0;
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

        private int freeFrom(int start, int length)              //the next of the last
        {
            for (int i = 0; i < length; ++i)
            {
                bitmap[start] = 0;
                start = disk.diskSpace[start];
            }
            return start;
        }
    }
}
