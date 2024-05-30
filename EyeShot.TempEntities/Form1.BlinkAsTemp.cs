using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeShot.TempEntities
{
    public partial class Form1 : Form
    {
        bool _showEntity = false;
        Timer _blinkTimer = null;
        SelectedItem _selectedItem = null;
        Mesh _blinkEntity = null;

        private void blinkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (blinkCheckBox.Checked)
            {
                blinkCheckBox.Text = "Disable";
                if (treeView1.SelectedNode == null)
                    treeView1.SelectedNode = treeView1.TopNode;

                StartBlink();
            }
            else
            {
                blinkCheckBox.Text = "Enable";
                StopBlink();
            }

            design1.Invalidate();
        }

        private void Blink(object sender, EventArgs e)
        {
            // 각 타이머 틱마다 교대로 장면에 임시 엔터티를 그립니다.
            _showEntity = !_showEntity;

            if (_showEntity)
                design1.TempEntities.Add(_blinkEntity);
            else
                design1.TempEntities.Remove(_blinkEntity);

            design1.Invalidate();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 트리뷰 선택에서 선택된 요소 인스턴스를 생성한다 
            _selectedItem = SynchTreeSelction(treeView1);

            if(blinkCheckBox.Checked)
            {
                StopBlink();

                StartBlink();
            }

            design1.Invalidate();
        }

        private SelectedItem SynchTreeSelction(TreeView tv)
        {
            // 노드 태그에서 시작하여 요소 및 블록 참조 스택을 채운다
            Stack<BlockReference> parents = new Stack<BlockReference>();

            TreeNode node = tv.SelectedNode;
            Entity entity = node.Tag as Entity;

            node = node.Parent;

            while (node != null)
            {
                var ent = node.Tag as Entity;
                if (ent != null)
                    parents.Push((BlockReference)ent);

                node = node.Parent;
            }

            tv.HideSelection = false;

            // 최상위 부모는 루트 블록 참조다 : 순서를 반대로하여 새로운 스택을 만들어야한다.
            Stack<BlockReference> stack = new Stack<BlockReference>(parents);

            return new SelectedItem(stack, entity);
        }

        private void StartBlink()
        {
            if (_selectedItem == null || _blinkEntity != null)
                return;

            // 선택된 요소로 부터 깜박이는 고유한 메시를 가져온다.
            _blinkEntity = GetUniqueEntity((Entity)_selectedItem.Item);

            // 이동중에 동기화를 유지하기 위해 깜박이는 임시 요소 참조를 저장한다. 
            if (_selectedItem.Item is BlockReference)
                ((Entity)_selectedItem.Item).EntityData = _blinkEntity;

            // 만약 선택된 아이템이 루트 요소가 아닌 경우, 루트를 찾는다
            if (_selectedItem.HasParents())
            {
                // 임시 엔터티를 원래 하위 엔터티의 실제 위치로 변환합니다.
                Transformation t = new Identity();
                foreach (BlockReference parent in _selectedItem.Parents)
                {
                    t = parent.Transformation * t;
                }

                _blinkEntity.TransformBy(t);

                // 반짝이는 임시 요소 참조를 루트 엘리먼트에 저장한다.
                _selectedItem.Parents.Last().EntityData = _blinkEntity;
            }

            _blinkEntity.Color = Color.FromArgb(100, Color.Yellow);

            // 반짝이는 요소의 모서리들 숨김처리
            _blinkEntity.Edges = null;
            _blinkEntity.EdgeStyle = Mesh.edgeStyleType.None;

            // 임시 요소를 그리는 것이 필요한 데이터를 계산한다.
            if (_blinkEntity.RegenMode == regenType.RegenAndCompile)
                _blinkEntity.Regen(0.1);

            _blinkTimer.Start();
        }

        private void StopBlink()
        {
            if (_blinkTimer == null)
                return;

            _blinkTimer.Stop();

            design1.TempEntities.Remove(_blinkEntity);

            _showEntity = false;
            _blinkEntity = null;
        }

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
