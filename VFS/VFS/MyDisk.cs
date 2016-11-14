using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS
{
    class MyDisk
    {
        public int[] diskSpace;
        public MyDisk(int block_size, int block_num)
        {
            diskSpace = new int[block_num];
        }
    }
}
