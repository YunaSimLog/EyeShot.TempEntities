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

        }
    }
}
