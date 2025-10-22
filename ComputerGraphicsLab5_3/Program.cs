using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ComputerGraphicsLab5_3
{
    public class BezierForm : Form
    {
        private List<PointF> controlPoints = new List<PointF>();
        private int selectedIndex = -1;
        private bool dragging = false;
        private const float rad = 6f;
        private float t = 0.5f; // параметр t

        private TrackBar tSlider;
        private Label tLabel;

        public BezierForm()
        {
            this.Text = "кубическая кривая Безье";
            this.DoubleBuffered = true;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            //параметр t
            tSlider = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickFrequency = 5,
                Dock = DockStyle.Bottom
            };
            tSlider.Scroll += (s, e) => { t = tSlider.Value / 100f; tLabel.Text = $"t = {t}"; Invalidate(); };

            tLabel = new Label
            {
                Text = "t = 0.50",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 20
            };

            Controls.Add(tLabel);
            Controls.Add(tSlider);

            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
            this.MouseMove += OnMouseMove;
            this.Paint += OnPaint;
            this.KeyDown += OnKeyDown;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int hit = HitTest(e.Location);
                if (hit >= 0)
                {
                    selectedIndex = hit;
                    dragging = true;
                }
                else
                {
                    controlPoints.Add(e.Location);
                    selectedIndex = controlPoints.Count - 1;
                    Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                int hit = HitTest(e.Location);
                if (hit >= 0)
                {
                    controlPoints.RemoveAt(hit);
                    selectedIndex = -1;
                    Invalidate();
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (dragging && selectedIndex >= 0 && selectedIndex < controlPoints.Count)
            {
                controlPoints[selectedIndex] = e.Location;
                Invalidate();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && selectedIndex >= 0 && selectedIndex < controlPoints.Count)
            {
                controlPoints.RemoveAt(selectedIndex);
                selectedIndex = -1;
                Invalidate();
            }
        }

        private int HitTest(PointF p)
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var d = Math.Sqrt(Math.Pow(controlPoints[i].X - p.X, 2) + Math.Pow(controlPoints[i].Y - p.Y, 2));
                if (d <= rad + 3) return i;
            }
            return -1;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (controlPoints.Count >= 4)
            {
                //Для каждой группы по 4 точки
                for (int i = 0; i <= controlPoints.Count - 4; i += 3)
                {
                    var segment = controlPoints.GetRange(i, 4);
                    Castel(g, segment, t);
                    BezierCurve(g, segment);
                }
            }
            else
            {
                g.DrawString("Добавьте ≥ 4 точки (лкм). Правый клик — удалить.",
                    this.Font, Brushes.Gray, 20, 20);
            }

            //контрольные точки
            for (int i = 0; i < controlPoints.Count; i++)
            {
                Brush b = (i == selectedIndex) ? Brushes.OrangeRed : Brushes.Green;
                g.FillEllipse(b, controlPoints[i].X - rad, controlPoints[i].Y - rad, rad * 2, rad * 2);
                g.DrawEllipse(Pens.Black, controlPoints[i].X - rad, controlPoints[i].Y - rad, rad * 2, rad * 2);
                g.DrawString($"P{i}", this.Font, Brushes.Black, controlPoints[i].X + 8, controlPoints[i].Y + 8);
            }
        }

        private void BezierCurve(Graphics g, List<PointF> pts)
        {
            const int steps = 100;
            PointF prev = CastelPoint(pts, 0);
            using (Pen pen = new Pen(Color.Blue, 2))
            {
                for (int i = 1; i <= steps; i++)
                {
                    float tt = i / (float)steps;
                    PointF cur = CastelPoint(pts, tt);
                    g.DrawLine(pen, prev, cur);
                    prev = cur;
                }
            }
        }

        private void Castel(Graphics g, List<PointF> pts, float t)
        {
            using (Pen polyPen = new Pen(Color.Gray, 1))
                g.DrawLines(polyPen, pts.ToArray());

            List<PointF> level1 = new List<PointF>();
            for (int i = 0; i < 3; i++)
                level1.Add(Linpol(pts[i], pts[i + 1], t));
            HelperLevel(g, level1, Brushes.Red, "¹");

            List<PointF> level2 = new List<PointF>();
            for (int i = 0; i < 2; i++)
                level2.Add(Linpol(level1[i], level1[i + 1], t));
            HelperLevel(g, level2, Brushes.Brown, "²");

            List<PointF> level3 = new List<PointF>();
            level3.Add(Linpol(level2[0], level2[1], t));
            HelperLevel(g, level3, Brushes.Blue, "³");

            using (Pen p1 = new Pen(Color.Red, 1))
                g.DrawLines(p1, level1.ToArray());
            using (Pen p2 = new Pen(Color.Brown, 1))
                g.DrawLines(p2, level2.ToArray());

            PointF bezierPoint = level3[0];
            g.FillEllipse(Brushes.DarkBlue, bezierPoint.X - 5, bezierPoint.Y - 5, 10, 10);
        }

        private void HelperLevel(Graphics g, List<PointF> pts, Brush brush, string sup)
        {
            using (Font f = new Font("Arial", 9))
            {
                for (int i = 0; i < pts.Count; i++)
                {
                    g.FillEllipse(brush, pts[i].X - 3, pts[i].Y - 3, 6, 6);
                    g.DrawEllipse(Pens.Black, pts[i].X - 3, pts[i].Y - 3, 6, 6);
                    g.DrawString($"P{i}{sup}", f, brush, pts[i].X + 6, pts[i].Y + 4);
                }
            }
        }

        private PointF Linpol(PointF a, PointF b, float t)
        {
            return new PointF(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        private PointF CastelPoint(List<PointF> pts, float t)
        {
            List<PointF> temp = new List<PointF>(pts);
            for (int k = 1; k < pts.Count; k++)
            {
                for (int i = 0; i < pts.Count - k; i++)
                {
                    temp[i] = Linpol(temp[i], temp[i + 1], t);
                }
            }
            return temp[0];
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new BezierForm());
        }
    }
}
