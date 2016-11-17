using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFS
{
    public class DirNode : ICloneable
    {
        public static int idCnt = 4;
        public int ID { get; set; }
        public string Name { get; set; }
        public int ParentID { get; set; }
        public List<DirNode> Nodes { get; set; }

        public DirNode()
        {
            this.Nodes = new List<DirNode>();
            this.ParentID = -1;
        }

        public DirNode(int pid, string name)
        {
            this.Nodes = new List<DirNode>();
            this.ID = idCnt++;
            this.ParentID = pid;
            this.Name = name;
        }

        private DirNode(int id, int pid, string name, List<DirNode> nodes)
        {
            this.ID = id;
            this.ParentID = pid;
            this.Name = (string)name.Clone();
            this.Nodes = new List<DirNode>();
            foreach (DirNode s in nodes)
            {
                this.Nodes.Add((DirNode)s.Clone());
            }
        }

        public Object Clone()
        {
            return new DirNode(ID + idCnt, ParentID + idCnt, Name, Nodes);
        }

        public static List<DirNode> BindDir(List<DirNode> nodes)
        {
            List<DirNode> outputList = new List<DirNode>();
            for (int i = 0; i < nodes.Count; ++i)
            {
                if (nodes[i].ParentID == -1)
                {
                    outputList.Add(nodes[i]);
                }
                else
                {
                    GetFather(nodes, nodes[i].ParentID).Nodes.Add(nodes[i]);
                }
            }
            return outputList;
        }

        private static DirNode GetFather(List<DirNode> nodes, int id)
        {
            if (nodes == null) return null;
            for (int i = 0; i < nodes.Count; ++i)
            {
                if (nodes[i].ID == id)
                    return nodes[i];
                DirNode node = GetFather(nodes[i].Nodes, id);
                if (node != null)
                    return node;
            }
            return null;
        }

        public static List<string> FindPathByID(DirNode root, int id)
        {
            List<string> path = new List<string>();
            if (FindPath(ref path, root, id)) return path;
            return null;
        }

        private static bool FindPath(ref List<string> path, DirNode root, int id)
        {
            path.Add(root.Name);
            if (root.ID == id) return true;
            foreach (DirNode node in root.Nodes)
            {
                if (FindPath(ref path, node, id))
                    return true;
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }

        private static int findMaxID(DirNode node)
        {
            int maxId = node.ID;
            foreach (DirNode dirNode in node.Nodes)
            {
                int id = findMaxID(dirNode);
                if (maxId < id) maxId = id;
            }
            return maxId;
        }

        public void UpdateID()
        {
            idCnt = findMaxID(this) + 1;
        }
    }
}
