using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS
{
    class Utility
    {
        public static string getCurrentTime()
        {
            return DateTime.Now.ToLocalTime().ToString();
        }
    }
}
