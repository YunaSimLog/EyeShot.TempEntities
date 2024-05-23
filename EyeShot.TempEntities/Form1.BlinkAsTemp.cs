using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeShot.TempEntities
{
    public partial class Form1 : Form
    {

        private void PopulateTree(TreeView tv, IList<Entity> entList, BlockKeyedCollection blocks, TreeNode parentNode = null)
        {
            TreeNodeCollection nodes;

            if (parentNode == null)
            {
                tv.Nodes.Clear();
                nodes = tv.Nodes;
            }
            else
            {
                nodes = parentNode.Nodes;
            }

            tv.BeginUpdate();

            for (int i = 0; i < entList.Count; i++)
            {
                Entity entity = entList[i];
                if (entity is BlockReference)
                {
                    Block child;
                    string blockName = (entity as BlockReference).BlockName;

                    if (blocks.TryGetValue(blockName, out child))
                    {
                        TreeNode parentTN = new TreeNode(GetNodeName(blockName, i));
                        parentTN.Tag = entity;
                        parentTN.ImageIndex = 0;
                        parentTN.SelectedImageIndex = 0;

                        nodes.Add(parentTN);
                        PopulateTree(tv, child.Entities, blocks, parentTN);
                    }
                }
                else
                {
                    string type = entity.GetType().ToString().Split('.').LastOrDefault();
                    var node = new TreeNode(GetNodeName(type, i));
                    node.Tag = entity;
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                    nodes.Add(node);
                }
            }

            tv.EndUpdate();
        }

        private string GetNodeName(string name, int index)
        {
            return $"{name} {index}";
        }

    }
}
