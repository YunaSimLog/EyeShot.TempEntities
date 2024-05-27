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
        string _selBlockName;
        bool _isDragging;
        Point3D _dragFrom;
        Entity _tempEntity;
        BlockReference _currentRef;

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;

            _selBlockName = listView1.SelectedItems[0].Name;
            listView1.SelectedItems.Clear();

            if (!_isDragging)
            {
                listView1.DoDragDrop(sender, DragDropEffects.Copy);
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (string.IsNullOrEmpty(_selBlockName))
                return;

            e.Effect = DragDropEffects.Copy;

            if (!_isDragging)
            {
                _isDragging = true;

                design1.DoDragDrop(sender, DragDropEffects.Copy);
            }
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            // 리스트 뷰 내에서 끝날 경우, 드래깅 작업을 초기화 합니다.
            _isDragging = false;
        }

        private void design1_DragEnter(object sender, DragEventArgs e)
        {
            if (_isDragging && _tempEntity == null && !string.IsNullOrEmpty(_selBlockName))
            {
                e.Effect = DragDropEffects.Copy;

                // 블럭 데이터로 임시 요소를 만든다
                _currentRef = new BlockReference(_selBlockName);
                Entity temp = GetUniqueEntity(_currentRef);

                // 체크되어 있다면 축 정렬된 바운딩 박스 요소로만 보입니다. 
                if (bboxCheckBox.Checked)
                {
                    Size3D s = temp.BoxSize;
                    Point3D bm = temp.BoxMin;
                    temp = Mesh.CreateBox(s.X, s.Y, s.Z);
                    temp.Translate(bm.X, bm.Y, bm.Z);
                    temp.Regen(0.1);
                }

                // 뷰포트에 임시 요소를 추가합니다.
                design1.TempEntities.Add(temp, Color.FromArgb(100, design1.Blocks[_selBlockName].Entities[0].Color));
                _tempEntity = temp;

                // 임시 요소의 시작 위치 점을 저장한다.
                _dragFrom = Plane.XY.PointAt(Plane.XY.Project((temp.BoxMax + temp.BoxMin) / 2));

                design1.Invalidate();
            }
            else
                e.Effect = DragDropEffects.None;
        }


        private void design1_DragOver(object sender, DragEventArgs e)
        {
            // 현재 마우스 위치를 가져온다
            System.Drawing.Point mouseLocation = design1.PointToClient(Cursor.Position);

            if (design1.ActionMode != devDept.Eyeshot.actionType.None || design1.ActiveViewport.ToolBar.Contains(mouseLocation))
                return;

            if (!_isDragging || _tempEntity == null)
                return;

            Point3D dragTo;

            // 마우스 좌표 위치를 뷰포트 좌표로 변환
            design1.ScreenToPlane(mouseLocation, Plane.XY, out dragTo);

            Vector3D delta = Vector3D.Subtract(dragTo, _dragFrom);

            // 임시 요소를 변환 적용
            _tempEntity.Translate(delta);
            _tempEntity.Regen(0.1);

            if (_tempEntity.EntityData == null)
                _tempEntity.EntityData = delta;
            else
                _tempEntity.EntityData = ((Vector3D)_tempEntity.EntityData) + delta;

            design1.TempEntities.UpdateBoundingBox();

            design1.Invalidate();

            _dragFrom = dragTo;
        }


        private void design1_DragDrop(object sender, DragEventArgs e)
        {
            if (!_isDragging)
                return;

            e.Effect = DragDropEffects.None;

            if (_selBlockName != null)
            {
                System.Drawing.Point mouseLocation = design1.PointToClient(Cursor.Position);

                Point3D dragTo;

                design1.ScreenToPlane(mouseLocation, Plane.XY, out dragTo);

                // 임시 요소의 현재 위치로 요소를 이동 시킨다.
                Vector3D delta = (Vector3D)_tempEntity.EntityData;
                _currentRef.Transformation = new Translation(delta);

                design1.Entities.Add(_currentRef);

                PopulateTree(treeView1, design1.Entities, design1.Blocks);
            }

            FinishDraggingOperation();
        }

        private void design1_DragLeave(object sender, EventArgs e)
        {
            if (_isDragging)
                FinishDraggingOperation();
        }

        private void FinishDraggingOperation()
        {
            // 임시 요소를 뷰 포트에서 제거
            design1.TempEntities.Remove(_tempEntity);

            _selBlockName = null;
            _currentRef = null;
            _tempEntity = null;
            _isDragging = false;

            design1.Invalidate();
        }
    }

}
