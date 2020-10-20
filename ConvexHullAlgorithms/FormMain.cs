using ConvexHullAlgorithms.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvexHullAlgorithms {
    public partial class FormMain : Form {
        private enum PointsShapes {
            Random,
            Circle,
            Rectangle
        }
        private PointsShapes pointsShape = PointsShapes.Random;
        private int psIdx = 0;
        private Array psValues;

        private int defaultPointsCount = 250;
        private int pointsCount;
        private PointF[] points;
        private PointF[] p;

        private List<(string Name, AlgorithmBase Algorithm)> abs;
        private int abIdx = 0;
        private CancellationTokenSource ct;
        private string elapsed = "";
        private int ps = 6;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            this.Resize += (_, __) => GenerateRandomPoints();
            this.Paint += Draw;
            this.KeyDown += HandleKeyDown;

            psValues = Enum.GetValues(typeof(PointsShapes));
        }

        private void HandleKeyDown(object sender, KeyEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Left:
                    if(abIdx == 0) return;
                    abIdx--;
                    break;
                case Keys.Right:
                    if(abIdx == abs.Count - 1) return;
                    abIdx++;
                    break;
                case Keys.Up:
                    if(psIdx == psValues.Length - 1) return;
                    pointsShape = (PointsShapes)psValues.GetValue(++psIdx);
                    GenerateRandomPoints();
                    break;
                case Keys.Down:
                    if(psIdx == 0) return;
                    pointsShape = (PointsShapes)psValues.GetValue(--psIdx);
                    GenerateRandomPoints();
                    break;
                case Keys.Enter:
                    GenerateRandomPoints();
                    return;
                case Keys.Escape:
                    ct?.Cancel();
                    this.Close();
                    return;
                default:
                    return;
            }
            RunAlgorithm();
        }

        private void RunAlgorithm() {
            ct?.Cancel();
            ct = GetPoints();

            int n = 250;
            long t = DateTime.Now.Ticks;
            for(int i = 0; i < n; i++) abs[abIdx].Algorithm.Run();
            elapsed = $"{(DateTime.Now.Ticks - t) / (double)(1000 * n):N2} ms";
        }

        private CancellationTokenSource GetPoints() {
            CancellationTokenSource ct = new CancellationTokenSource();

            Task.Run(() => {
                int n = 0;
                while(true) {
                    int k = 0;
                    List<PointF> lp = new List<PointF>();
                    foreach(PointF[] pf in abs[abIdx].Algorithm.RunYield()) {
                        if(ct.IsCancellationRequested) return;
                        if(n == k++) {
                            Thread.Sleep(200);
                            lp.AddRange(pf);
                            p = lp.ToArray();
                            this.Invalidate();
                        }
                    }
                    n++;
                }
            }, ct.Token);

            return ct;
        }

        private void Draw(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            int ps2 = ps / 2;
            for(int i = 0; i < points.Length; i++)
                g.FillEllipse(Brushes.White, points[i].X - ps2, points[i].Y - ps2, ps, ps);

            //g.DrawClosedCurve(Pens.Red, p, 0, FillMode.Alternate);

            if(p?.Length > 1) using(Pen pc = new Pen(Color.OrangeRed, 2)) g.DrawLines(pc, p);

            int h = this.Font.Height;
            g.DrawString($"[{abIdx + 1}/{abs.Count}] {abs[abIdx].Name}", this.Font, Brushes.White, 5, 5 + h * 0);
            g.DrawString($"[{psIdx + 1}/{psValues.Length}] {(PointsShapes)psValues.GetValue(psIdx)}", this.Font, Brushes.LightGray, 5, +h * 1 + 5);

            g.DrawString($"Elapsed:    {elapsed}", this.Font, Brushes.Yellow, 5, h * 2 + 10);
            g.DrawString($"Points:     {p?.Length} / {pointsCount}", this.Font, Brushes.Yellow, 5, h * 3 + 10);
            g.DrawString($"Left/Right: Change Algorithm", this.Font, Brushes.Gray, 5, h * 4 + 10);
            g.DrawString($"Up/Down:    Change Points Shape", this.Font, Brushes.Gray, 5, h * 5 + 10);
            g.DrawString($"Enter:      Randomize Points", this.Font, Brushes.Gray, 5, h * 6 + 10);
            g.DrawString($"Escape:     Close Program", this.Font, Brushes.Gray, 5, h * 7 + 10);
        }

        private void GenerateRandomPoints() {
            points = new PointF[defaultPointsCount];

            Random rnd = new Random();

            List<PointF> pts = new List<PointF>();
            double xo = this.DisplayRectangle.Width * 0.1;
            double yo = this.DisplayRectangle.Height * 0.1;
            double w = this.DisplayRectangle.Width * 0.8;
            double h = this.DisplayRectangle.Height * 0.8;

            switch(pointsShape) {
                case PointsShapes.Random:
                    for(int i = 0; i < defaultPointsCount; i++) {
                        float x = (float)(xo + rnd.NextDouble() * w);
                        float y = (float)(yo + rnd.NextDouble() * h);
                        pts.Add(new PointF(x, y));
                    }
                    break;

                case PointsShapes.Circle:
                    w = this.DisplayRectangle.Width / 2;
                    h = this.DisplayRectangle.Height / 2;
                    int r = (int)(Math.Min(w, h) * 0.8);
                    for(int i = 0; i < 360; i += 10) {
                        float x = (float)(w + r * Math.Cos(i * Math.PI / 180.0));
                        float y = (float)(h + r * Math.Sin(i * Math.PI / 180.0));
                        pts.Add(new PointF(x, y));
                    }
                    break;

                case PointsShapes.Rectangle:
                    pts = new List<PointF>();
                    for(double x = xo; x < w; x += 40) {
                        pts.Add(new PointF((float)x, (float)yo));
                        pts.Add(new PointF((float)x, (float)h));
                    }
                    for(double y = yo; y < h; y += 40) {
                        pts.Add(new PointF((float)xo, (float)y));
                        pts.Add(new PointF((float)w, (float)y));
                    }
                    break;
            }

            bool isDone;
            int psM = ps + 4;
            do {
                isDone = true;
                for(int i = 0; i < pts.Count; i++) {
                    for(int j = i + 1; j < pts.Count; j++) {
                        if(AlgorithmBase.Distance(pts[i], pts[j]) < psM) {
                            isDone = false;
                            pts.RemoveAt(j);
                            break;
                        }
                    }
                    if(!isDone) break;
                }
            } while(!isDone);
            points = pts.ToArray();
            pointsCount = pts.Count;

            abs = AlgorithmBase.GetAlgorithms(points);

            RunAlgorithm();
        }
    }
}