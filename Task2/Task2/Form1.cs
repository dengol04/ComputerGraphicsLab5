using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Task2
{
    public partial class Form1 : Form
    {
        private List<PointF> mountainPoints;
        private Random random = new Random();
        private int currentPos = 0;
        private int maxPos = 5;
        private float smoothing = 0.5f;

        public Form1()
        {
            InitializeComponent();
            pictureBox.Paint += new PaintEventHandler(pictureBox_Paint);
            GeneratePoints();
        }

        private void GeneratePoints()
        {
            mountainPoints = new List<PointF>
            {
                new PointF(0, pictureBox.Height / 2),
                new PointF(pictureBox.Width, pictureBox.Height / 2)
            };
            currentPos = 0;
        }

        private void NextStep()
        {
            if (currentPos >= maxPos) return;

            float distanceRange = (pictureBox.Height / 2.0f) * (float)Math.Pow(2, -currentPos * smoothing);

            for (int i = 0; i < mountainPoints.Count - 1; i += 2)
            {
                PointF p1 = mountainPoints[i];
                PointF p2 = mountainPoints[i + 1];

                PointF midPoint = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

                float distance = (float)(random.NextDouble() * 2 - 1) * distanceRange;
                midPoint.Y += distance;

                midPoint.Y = Math.Max(0, midPoint.Y);
                midPoint.Y = Math.Min(pictureBox.Height, midPoint.Y);

                mountainPoints.Insert(i + 1, midPoint);
            }
            currentPos++;
            pictureBox.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            maxPos = (int)numericUpDown.Value;
            smoothing = trackBar.Value / 10.0f;

            GeneratePoints();
            pictureBox.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NextStep();
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(GetPoints());

                using (SolidBrush brush = new SolidBrush(Color.DarkSlateGray))
                {
                    e.Graphics.FillPath(brush, path);
                }

                using (Pen pen = new Pen(Color.Black, 2))
                {
                    e.Graphics.DrawLines(pen, mountainPoints.ToArray());
                }
            }
        }

        private PointF[] GetPoints()
        {
            List<PointF> Points = new List<PointF>(mountainPoints)
            {
                new PointF(pictureBox.Width, pictureBox.Height),
                new PointF(0, pictureBox.Height)
            };
            return Points.ToArray();
        }
    }
}
