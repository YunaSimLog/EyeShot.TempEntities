using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace EyeShot.TempEntities
{
    public partial class Form1 : Form
    {
        private readonly string _dirName = "myPictures";
        private Plane _plane;

        public Form1()
        {
            InitializeComponent();

        }

        protected override void OnLoad(EventArgs e)
        {
            CreateElements();

            base.OnLoad(e);
        }

        private void CreateElements()
        {
            Color oldColor = (design1.ActiveViewport.Background).TopColor;
            design1.Backface.ColorMethod = devDept.Graphics.backfaceColorMethodType.SingleColor;
            design1.ActiveViewport.Background.TopColor = Color.White;
            design1.Flat.EdgeThickness = 10;
            design1.Flat.SilhouetteThickness = 10;

            design1.ActiveViewport.DisplayMode = displayType.Flat;
            design1.Flat.ColorMethod = flatColorMethodType.EntityMaterial;

            design1.SetView(viewType.Trimetric);

            // 재질 요소를 저장 하기 위한 디렉토리 생성 및 초기화
            if (!Directory.Exists(_dirName))
                Directory.CreateDirectory(_dirName);
            else
            {
                foreach (string filePath in Directory.GetFiles(_dirName))
                    File.Delete(filePath);
            }

            // 평면 초기화
            _plane = new Plane();

            // 객체의 색상과 재질 정의
            Bitmap img = new Bitmap(@"C:\Program Files\devDept Software\Eyeshot 2023\Samples\dataset\Assets\Textures\Maple.jpg");
            Material m = img.CreateMaterial("wood");
            m.TextureLength = 60;
            design1.Materials.Add(m);

            Color[] colors = new Color[]
            {
                Color.Gray,
                Color.FromArgb(255, 0xF9, 0X88, 0x66),
                Color.FromArgb(255, 0xFF, 0X42, 0x0E),
                Color.FromArgb(255, 0x80, 0XBD, 0x9E),
                Color.FromArgb(255, 0x89, 0XDA, 0x59),
            };

            Entity baseMesh, slotMesh, triangleMesh, cylMesh, wheelAxisMesh, wheelLMesh, wheelRMesh;

            // slot
            devDept.Eyeshot.Entities.Region slot = devDept.Eyeshot.Entities.Region.CreateRoundedRectangle(60, 20, 5, true);
            devDept.Eyeshot.Entities.Region circle = devDept.Eyeshot.Entities.Region.CreateCircle(3.6);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            circle.Translate(-20, 0, 0);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            circle.Translate(40, 0, 0);
            slot = devDept.Eyeshot.Entities.Region.Difference(slot, circle)[0];

            slotMesh = slot.ExtrudeAsBrep(Vector3D.AxisZ * 5);
            slotMesh.Rotate(Math.PI / 2, Vector3D.AxisZ);
            slotMesh.Color = colors[0];
            slotMesh.MaterialName = "wood";
            slotMesh.ColorMethod = colorMethodType.byEntity;

            // triangle
            LinearPath trianglePath = new LinearPath(Point3D.Origin, new Point3D(36, 0, 0), new Point3D(18, 0, 25), Point3D.Origin);
            devDept.Eyeshot.Entities.Region triangleRegion2 = new devDept.Eyeshot.Entities.Region(trianglePath);
            triangleMesh = triangleRegion2.ExtrudeAsBrep(Vector3D.AxisMinusY * 5);
            triangleMesh.Color = colors[1];
            triangleMesh.ColorMethod = colorMethodType.byEntity;
            triangleMesh.Rotate(Utility.DegToRad(90), Vector3D.AxisMinusZ);
            triangleMesh.Translate(52, -3, 0);

            // wheels
            wheelAxisMesh = Brep.CreateCylinder(3, 65);
            wheelAxisMesh.MaterialName = "wood";
            wheelAxisMesh.Rotate(Math.PI / 2, Vector3D.AxisY);
            wheelAxisMesh.Color = colors[2];
            wheelAxisMesh.ColorMethod = colorMethodType.byEntity;

            devDept.Eyeshot.Entities.Region outer = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 12);
            devDept.Eyeshot.Entities.Region inner = devDept.Eyeshot.Entities.Region.CreateCircle(Plane.YZ, 3);
            devDept.Eyeshot.Entities.Region wheel = devDept.Eyeshot.Entities.Region.Difference(outer, inner)[0];

            wheelRMesh = wheel.ExtrudeAsBrep(10);
            wheelRMesh.Translate(55, 0, 0);
            wheelRMesh.Color = colors[2];
            wheelRMesh.ColorMethod = colorMethodType.byEntity;

            wheelLMesh = wheel.ExtrudeAsBrep(-10);
            wheelLMesh.Translate(10, 0, 0);
            wheelLMesh.Color = colors[2];
            wheelLMesh.ColorMethod = colorMethodType.byEntity;

            // cylinder
            cylMesh = Brep.CreateCylinder(3.5, 40);
            cylMesh.Color = colors[3];
            cylMesh.ColorMethod = colorMethodType.byEntity;

            // box
            baseMesh = Brep.CreateBox(40, 40, 5);
            baseMesh.Color = colors[4];
            baseMesh.ColorMethod = colorMethodType.byEntity;



        }
    }
}
