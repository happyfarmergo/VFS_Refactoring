using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace VFS
{
    [Serializable]
    class MyDisk
    {
        public int[] diskSpace;
        public MyDisk(int block_size, int block_num)
        {
            diskSpace = new int[block_num];
        }
    }
}
