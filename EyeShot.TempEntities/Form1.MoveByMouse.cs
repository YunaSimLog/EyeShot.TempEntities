using System;
using System.Drawing;
using System.Windows.Forms;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace EyeShot.TempEntities
{
	public partial class Form1 : Form
	{
		Plane _plane;
		Point3D _centerOfArrows;
		Mesh[] _tempArrows;

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

			Vector3D diagonalV = new Vector3D(_tempArrows[0].BoxMax, _tempArrows[0].BoxMax);
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

