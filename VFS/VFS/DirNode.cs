using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VFS
{
    [Serializable]
    public class DirNode : ICloneable
    {
        public string Name { get; set; }
        public DirNode Parent { get; set; }
        public List<DirNode> Nodes { get; set; }

        public DirNode()
        {
            this.Nodes = new List<DirNode>();
        }

        public DirNode(string name, DirNode parent = null)
        {
            this.Nodes = new List<DirNode>();
            this.Parent = parent;
            this.Name = name;
        }

        private DirNode(string name, DirNode parent, List<DirNode> nodes)
        {
            this.Name = (string)name.Clone();
            this.Nodes = new List<DirNode>();
            this.Parent = parent;
            DirNode tmp = null;
            foreach (DirNode s in nodes)
            {
                tmp = (DirNode)s.Clone();
                this.Nodes.Add(tmp);
                tmp.Parent = this;
            }
        }

        public Object Clone()
        {
            return new DirNode(Name, Parent, Nodes);
        }

        public static List<string> FindPath(DirNode node)
        {
            DirNode root = null;
            List<string> path = new List<string>();
            path.Add(node.Name);
            while ((root = node.Parent) != null)
            {
                path.Add(root.Name);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

    }
}
