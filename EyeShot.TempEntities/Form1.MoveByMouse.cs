using System;
using System.Drawing;
using System.Windows.Forms;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace EyeShot.TempEntities
{
    public partial class Form1 : Form
    {
        int _entitiyIndex = -1;
        bool _move = false;
        Plane _plane;
        Point3D _moveForm;
        Point3D _centerOfArrows;
        Mesh[] _tempArrows;

        private void moveCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (moveCheckBox.Checked)
            {
                moveCheckBox.Text = "Disable";
                _move = true;
            }
            else
            {
                moveCheckBox.Text = "Enable";
                _move = false;
            }
        }

        private void planeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (planeCombo.SelectedIndex)
            {
                case 0:
                    _plane = Plane.XY;
                    break;
                case 1:
                    _plane = Plane.ZX;
                    break;
                case 2:
                    _plane = Plane.YZ;
                    break;
                default:
                    _plane = Plane.XY;
                    break;
            }

            CreateArrowsDirections();
        }

        private void design1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_move || e.Button != MouseButtons.Left || design1.ActionMode != devDept.Eyeshot.actionType.None || design1.ActiveViewport.ToolBar.Contains(e.Location))
                return;

            _entitiyIndex = design1.GetEntityUnderMouseCursor(e.Location);

            if (_entitiyIndex < 0)
                return;

            // 3D 시작 점을 가져온다
            design1.ScreenToPlane(e.Location, _plane, out _moveForm);
        }

        private void design1_MouseMove(object sender, MouseEventArgs e)
        {
            // 만약 이동 동작이 활성화된 경우, 마우스가 요소에 호버될 때 임시 화살표를 그린다
            if (_move && e.Button == MouseButtons.None)
                TranslateAndShowArrows();

            if (!_move || e.Button != MouseButtons.Left || design1.ActionMode != devDept.Eyeshot.actionType.None || design1.ActiveViewport.ToolBar.Contains(e.Location) || _entitiyIndex == -1)
                return;

            design1.TempEntities.Remove(_tempArrows[0]);
            design1.TempEntities.Remove(_tempArrows[1]);
            design1.TempEntities.Remove(_tempArrows[2]);
            design1.TempEntities.Remove(_tempArrows[3]);

            Entity entity = design1.Entities[_entitiyIndex] as Entity;

            Point3D moveTo;

            design1.ScreenToPlane(e.Location, _plane, out moveTo);

            Vector3D delta = Vector3D.Subtract(moveTo, _moveForm);

            entity.Translate(delta);

            design1.Entities.Regen();

            design1.Invalidate();

            _moveForm = moveTo;

            if (entity.EntityData != null)
            {
                ((Entity)entity.EntityData).Translate(delta);
                ((Entity)entity.EntityData).Regen(0.01);
            }
        }

        private void TranslateAndShowArrows()
        {
            System.Drawing.Point mouseLocation = design1.PointToClient(Cursor.Position);

            _entitiyIndex = design1.GetEntityUnderMouseCursor(mouseLocation);

            if (_entitiyIndex < 0)
            {
                if (_tempArrows != null)
                {
                    design1.TempEntities.Remove(_tempArrows[0]);
                    design1.TempEntities.Remove(_tempArrows[1]);
                    design1.TempEntities.Remove(_tempArrows[2]);
                    design1.TempEntities.Remove(_tempArrows[3]);
                }

                design1.Invalidate();
                return;
            }

            // 요소 바운딩 박스의 센터를 가져온다
            Entity entity = design1.Entities[_entitiyIndex];
            Point3D center = (entity.BoxMax + entity.BoxMin) / 2;

            // 화살표 센터 위치에서 부터 요소 중심 위치으로 변환을 가져온다
            Vector3D trans = new Vector3D(_centerOfArrows, center);

            _tempArrows[0].Translate(trans);
            _tempArrows[1].Translate(trans);
            _tempArrows[2].Translate(trans);
            _tempArrows[3].Translate(trans);

            _centerOfArrows = center;

            // 아직 추가하지 않았다면, 임시 요소 리스트에 추가한다.
            if (design1.TempEntities.Count < 4)
            {
                design1.TempEntities.Add(_tempArrows[0]);
                design1.TempEntities.Add(_tempArrows[1]);
                design1.TempEntities.Add(_tempArrows[2]);
                design1.TempEntities.Add(_tempArrows[3]);

                design1.TempEntities.UpdateBoundingBox();
            }

            design1.Invalidate();
        }

        private void CreateArrowsDirections()
        {
            // 이전 화살표가 존재하는 경우 제거한다.
            if (_tempArrows != null)
            {
                design1.TempEntities.Remove(_tempArrows[0]);
                design1.TempEntities.Remove(_tempArrows[1]);
                design1.TempEntities.Remove(_tempArrows[2]);
                design1.TempEntities.Remove(_tempArrows[3]);
            }

            // 현재 움직이는 평면에 4개의 임시 화살표를 생성하여, 마우스가 요소 위에 있을 때 표시합니다. 
            _tempArrows = new Mesh[4];

            devDept.Eyeshot.Entities.Region arrowShape = new devDept.Eyeshot.Entities.Region(new LinearPath(_plane, new Point2D[]
            {
                new Point2D(0,-2),
                new Point2D(4,-2),
                new Point2D(4,-4),
                new Point2D(10,0),
                new Point2D(4,4),
                new Point2D(4,2),
                new Point2D(0,2),
                new Point2D(0,-2),
            }), _plane);

            //  우측 화살표
            _tempArrows[0] = arrowShape.ExtrudeAsMesh(2, 0.1, Mesh.natureType.Plain);
            _tempArrows[0].Regen(0.1);
            _tempArrows[0].Color = Color.FromArgb(180, Color.Red);

            // 위 화살표
            _tempArrows[1] = (Mesh)_tempArrows[0].Clone();
            _tempArrows[1].Rotate(Math.PI / 2, _plane.AxisZ);
            _tempArrows[1].Regen(0.1);

            // 좌측 화살표
            _tempArrows[2] = (Mesh)_tempArrows[0].Clone();
            _tempArrows[2].Rotate(Math.PI, _plane.AxisZ);
            _tempArrows[2].Regen(0.1);

            // 아래 화살표
            _tempArrows[3] = (Mesh)_tempArrows[0].Clone();
            _tempArrows[3].Rotate(-Math.PI / 2, _plane.AxisZ);
            _tempArrows[3].Regen(0.1);

            Vector3D diagonalV = new Vector3D(_tempArrows[0].BoxMin, _tempArrows[0].BoxMax);
            double offset = Math.Max(Vector3D.Dot(diagonalV, _plane.AxisX), Vector3D.Dot(diagonalV, _plane.AxisY));
            Vector3D translateX = _plane.AxisX * offset / 2;
            Vector3D translateY = _plane.AxisY * offset / 2;

            _tempArrows[0].Translate(translateX);
            _tempArrows[1].Translate(translateY);
            _tempArrows[2].Translate(-1 * translateX);
            _tempArrows[3].Translate(-1 * translateY);

            _centerOfArrows = Point3D.Origin;
        }
    }
}

