using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Globalization;
using System.Drawing.Drawing2D;

namespace Task1
{
    public partial class Form1 : Form
    {
        private Random random = new Random();

        public Form1()
        {
            InitializeComponent();
            this.Text = "L-System Фракталы";
            this.txtFilePath.Text = "bush.txt";
            this.txtFilePath.ReadOnly = true;
        }

        public class TurtleState
        {
            public PointF Position { get; set; }
            public double Direction { get; set; }
            public float Thickness { get; set; }
            public Color Color { get; set; }
            public int Depth { get; set; }
        }

        public class LSystem
        {
            public string Axiom { get; set; }
            public double Angle { get; set; }
            public double InitialDirection { get; set; }
            public Dictionary<char, string> Rules { get; set; }

            public LSystem()
            {
                Rules = new Dictionary<char, string>();
            }

            public static LSystem LoadFromFile(string filePath)
            {
                var lsys = new LSystem();
                var lines = File.ReadAllLines(filePath);

                var parts = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                lsys.Axiom = parts[0].Trim();
                lsys.Angle = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                lsys.InitialDirection = double.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var ruleParts = lines[i].Split('=');
                    if (ruleParts.Length == 2)
                    {
                        lsys.Rules.Add(ruleParts[0].Trim()[0], ruleParts[1].Trim());
                    }
                }
                return lsys;
            }

            public string Generate(int iterations)
            {
                string current = Axiom;
                for (int i = 0; i < iterations; i++)
                {
                    var next = new StringBuilder();
                    foreach (char c in current)
                    {
                        if (Rules.ContainsKey(c))
                        {
                            next.Append(Rules[c]);
                        }
                        else
                        {
                            next.Append(c);
                        }
                    }
                    current = next.ToString();
                }
                return current;
            }
        }

        private Color LerpColor(Color startColor, Color endColor, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            int r = (int)(startColor.R + (endColor.R - startColor.R) * t);
            int g = (int)(startColor.G + (endColor.G - startColor.G) * t);
            int b = (int)(startColor.B + (endColor.B - startColor.B) * t);
            return Color.FromArgb(r, g, b);
        }

        private void DrawFractal(Graphics g, LSystem lsys, string generatedString)
        {
            if (generatedString.Length == 0) return;

            float stepLength = 10.0f;

            var stateStack = new Stack<TurtleState>();
            var currentState = new TurtleState
            {
                Position = new PointF(0, 0),
                Direction = lsys.InitialDirection
            };

            float minX = currentState.Position.X, minY = currentState.Position.Y;
            float maxX = currentState.Position.X, maxY = currentState.Position.Y;

            foreach (char symbol in generatedString)
            {
                switch (symbol)
                {
                    case 'F':
                    case 'f':
                        {
                            float rad = (float)(currentState.Direction * Math.PI / 180.0);
                            float dx = (float)(stepLength * Math.Cos(rad));
                            float dy = (float)(stepLength * Math.Sin(rad));
                            currentState.Position = new PointF(currentState.Position.X + dx, currentState.Position.Y - dy);

                            minX = Math.Min(minX, currentState.Position.X);
                            maxX = Math.Max(maxX, currentState.Position.X);
                            minY = Math.Min(minY, currentState.Position.Y);
                            maxY = Math.Max(maxY, currentState.Position.Y);
                            break;
                        }
                    case '+':
                        currentState.Direction += lsys.Angle;
                        break;
                    case '-':
                        currentState.Direction -= lsys.Angle;
                        break;
                    case '[':
                        stateStack.Push(new TurtleState { Position = currentState.Position, Direction = currentState.Direction });
                        break;
                    case ']':
                        if (stateStack.Count > 0) currentState = stateStack.Pop();
                        break;
                }
            }

            float width = maxX - minX;
            float height = maxY - minY;
            float margin = 20.0f;

            if (width < 1.0f) width = 1.0f;
            if (height < 1.0f) height = 1.0f;

            float scaleFactorX = (pictureBoxCanvas.Width - 2 * margin) / width;
            float scaleFactorY = (pictureBoxCanvas.Height - 2 * margin) / height;

            float scale = Math.Min(scaleFactorX, scaleFactorY) * 0.95f;

            float finalStepLength = stepLength * scale;

            float offsetX = margin - minX * scale;
            float offsetY = margin - minY * scale;

            stateStack.Clear();

            bool isBranching = generatedString.Contains("[") && generatedString.Contains("]");

            float maxDrawingDepth = isBranching ? (float)numIterations.Value : 1.0f;

            float initialThickness = isBranching ? 3.0f * scale : 1.0f;
            Color defaultColor = isBranching ? Color.SaddleBrown : Color.White;
            Color brown = Color.SaddleBrown;
            Color green = Color.ForestGreen;

            currentState = new TurtleState
            {
                Position = new PointF(offsetX + 0 * scale, offsetY + 0 * scale),
                Direction = lsys.InitialDirection,
                Thickness = initialThickness,
                Color = defaultColor,
                Depth = 0
            };

            foreach (char symbol in generatedString)
            {
                switch (symbol)
                {
                    case 'F':
                        {
                            float rad = (float)(currentState.Direction * Math.PI / 180.0);
                            float dx = (float)(finalStepLength * Math.Cos(rad));
                            float dy = (float)(finalStepLength * Math.Sin(rad));
                            PointF nextPos = new PointF(currentState.Position.X + dx, currentState.Position.Y - dy);

                            if (currentState.Thickness > 0.0f)
                            {
                                using (var pen = new Pen(currentState.Color, currentState.Thickness))
                                {
                                    pen.StartCap = isBranching ? LineCap.Round : LineCap.Flat;
                                    pen.EndCap = isBranching ? LineCap.Round : LineCap.Flat;
                                    g.DrawLine(pen, currentState.Position, nextPos);
                                }
                            }

                            currentState.Position = nextPos;

                            if (isBranching)
                            {
                                currentState.Thickness *= 0.85f;
                                if (currentState.Thickness < 0.5f) currentState.Thickness = 0.5f;

                                float t = currentState.Depth / maxDrawingDepth;
                                currentState.Color = LerpColor(brown, green, t);
                            }
                            break;
                        }
                    case 'f':
                        {
                            float rad = (float)(currentState.Direction * Math.PI / 180.0);
                            float dx = (float)(finalStepLength * Math.Cos(rad));
                            float dy = (float)(finalStepLength * Math.Sin(rad));
                            currentState.Position = new PointF(currentState.Position.X + dx, currentState.Position.Y - dy);
                            break;
                        }
                    case '+':
                        {
                            if (isBranching)
                            {
                                double angleDelta = lsys.Angle;
                                angleDelta += (random.NextDouble() * 2 - 1) * 10.0;
                                currentState.Direction += angleDelta;
                            }
                            else
                            {
                                currentState.Direction += lsys.Angle;
                            }
                            break;
                        }
                    case '-':
                        {
                            if (isBranching)
                            {
                                double angleDelta = lsys.Angle;
                                angleDelta += (random.NextDouble() * 2 - 1) * 10.0;
                                currentState.Direction -= angleDelta;
                            }
                            else
                            {
                                currentState.Direction -= lsys.Angle;
                            }
                            break;
                        }
                    case '[':
                        {
                            stateStack.Push(new TurtleState
                            {
                                Position = currentState.Position,
                                Direction = currentState.Direction,
                                Thickness = currentState.Thickness,
                                Color = currentState.Color,
                                Depth = currentState.Depth
                            });
                            currentState.Depth++;
                            break;
                        }
                    case ']':
                        {
                            if (stateStack.Count > 0)
                            {
                                currentState = stateStack.Pop();
                            }
                            break;
                        }
                }
            }
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Пожалуйста, выберите существующий файл L-системы.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                LSystem lsys = LSystem.LoadFromFile(txtFilePath.Text);
                int iterations = (int)numIterations.Value;

                string fractalString = lsys.Generate(iterations);

                var bmp = new Bitmap(pictureBoxCanvas.Width, pictureBoxCanvas.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Black);

                    DrawFractal(g, lsys, fractalString);
                }

                pictureBoxCanvas.Image = bmp;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Файл L-системы не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog.Filter = "L-System Files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
            }
        }
    }
}