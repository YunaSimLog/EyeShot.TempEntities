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
        private enum ITEM_TYPE
        {
            Vertex,
            Edge,
            Face,
            None
        }

        ITEM_TYPE _itemMode = ITEM_TYPE.None;

        List<Entity> _tempItems = new List<Entity>();

        private void addItemCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectionFilterMode();
        }

        private void addItemCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetSelectionFilterMode();

            if (addItemCheckBox.Checked)
            {
                addItemCheckBox.Text = "Disable";

                // 마우스 커서 아래의 leaf Brep 요소를 가져온다
                design1.AssemblySelectionMode = devDept.Eyeshot.Control.Workspace.assemblySelectionType.Leaf;

                // 이동 동작 및 버튼을 비활성화한다.
                _move = false;
                moveCheckBox.Enabled = false;
            }
            else
            {
                addItemCheckBox.Text = "Enable";

                // 임시 아이템을 초기화한다.
                foreach (Entity item in _tempItems)
                    design1.TempEntities.Remove(item);
                _tempItems.Clear();

                // 이동 동작 및 버튼을 복구한다.
                moveCheckBox.Enabled = true;
                _move = moveCheckBox.Checked;
            }

            design1.Invalidate();
        }

        private void design1_MouseUp(object sender, MouseEventArgs e)
        {
            // 화면의 임시 요소로 마우스 커서 아래에 항목을 추가합니다.
            if (e.Button == MouseButtons.Left && design1.ActionMode == devDept.Eyeshot.actionType.None && !design1.ActiveViewport.ToolBar.Contains(e.Location))
                AddEntityItem(e.Location);
        }

        private void AddEntityItem(System.Drawing.Point mousePosition)
        {
            if (_itemMode == ITEM_TYPE.None)
                return;

            // 부모 블록 참조의 변환
            Transformation transformation = new Identity();

            // 임시 요소 리스트에 추가할 마우스 커서 아래 아이템
            Entity tempItem = null;

            // 마우스 아래 꼭지점 가져오기
            SelectedSubItem selItem = (SelectedSubItem)design1.GetItemUnderMouseCursor(mousePosition);
            if (selItem == null)
                return;

            Brep brep = (Brep)selItem.Item;

            // 부모 블록의 변환을 가져온다.
            transformation = selItem.Parents.First().Transformation;

            switch (_itemMode)
            {
                case ITEM_TYPE.Vertex:
                    // 꼭지점 아이템을 나타내는 점을 임시요소로 생성한다.
                    tempItem = new devDept.Eyeshot.Entities.Point(brep.Vertices[selItem.Index], 15);
                    tempItem.Color = Color.FromArgb(150, Color.Blue);
                    break;
                case ITEM_TYPE.Edge:
                    // 모서리 아이템을 나타내는 ICurve를 임시요소로 생성한다
                    tempItem = (Entity)brep.Edges[selItem.Index].Curve.Clone();
                    tempItem.LineWeight = 10;
                    tempItem.Color = Color.FromArgb(150, Color.Purple);
                    break;
                case ITEM_TYPE.Face:
                    // 면 아이템을 나타내는 Mesh를 임시요소로 생성한다.
                    tempItem = brep.Faces[selItem.Index].ConvertToMesh(skipEdges: true);
                    tempItem.Color = Color.FromArgb(150, Color.DeepSkyBlue);
                    break;
            }

            // 임시 요소를 표시된 항목으로 변환한다.
            tempItem.TransformBy(transformation);

            if (tempItem is ICurve)
                tempItem.Regen(0.1);

            design1.TempEntities.Add(tempItem);
            
            _tempItems.Add(tempItem);
        }

        private void SetSelectionFilterMode()
        {
            if (addItemCheckBox.Checked)
            {
                _itemMode = (ITEM_TYPE)addItemCombo.SelectedIndex;

                switch (_itemMode)
                {
                    case ITEM_TYPE.Vertex:
                        // 마우스 커서 아래의 꼭지점만 가져오도록 선택 필터 모드를 설정합니다.
                        design1.SelectionFilterMode = devDept.Eyeshot.selectionFilterType.Vertex;
                        break;
                    case ITEM_TYPE.Edge:
                        // 마우스 커서 아래의 모서리만 가져오도록 선택 필터 모드를 설정합니다.
                        design1.SelectionFilterMode = devDept.Eyeshot.selectionFilterType.Edge;
                        break;
                    case ITEM_TYPE.Face:
                        // 마우스 커서 아래의 면만 가져오도록 선택 필터 모드를 설정합니다.
                        design1.SelectionFilterMode = devDept.Eyeshot.selectionFilterType.Face;
                        break;
                }
            }
            else
                _itemMode = ITEM_TYPE.None;
        }
    }
}
