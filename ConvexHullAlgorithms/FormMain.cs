using ConvexHullAlgorithms.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvexHullAlgorithms {
    public partial class FormMain : Form {
        private int pointsCount = 100;
        private PointF[] points;
        private PointF[] p;

        private List<(string Name, AlgorithmBase Algorithm)> abs;
        private int abIdx = 0;
        private CancellationTokenSource ct;

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            this.Resize += (_, __) => GenerateRandomPoints();
            this.Paint += Draw;
            this.KeyDown += SwitchAlgorithm;
        }

        private void SwitchAlgorithm(object sender, KeyEventArgs e) {
            switch(e.KeyCode) {
                case Keys.Left:
                    if(abIdx == 0) return;
                    abIdx--;
                    break;
                case Keys.Right:
                    if(abIdx == abs.Count - 1) return;
                    abIdx++;
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
                            Thread.Sleep(250);
                            lp.AddRange(pf);
                            p = lp.ToArray();
                            this.Invalidate();
                        }
                    }
                    n++;
                }
            }, ct.Token);

            //p = ab.Run();
            //this.Invalidate();

            return ct;
        }

        private void Draw(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;

            for(int i = 0; i < pointsCount; i++)
                g.FillEllipse(Brushes.White, points[i].X - 3, points[i].Y - 3, 6, 6);

            //g.DrawClosedCurve(Pens.Red, p, 0, FillMode.Alternate);

            if(p?.Length >= 2) g.DrawLines(Pens.Red, p);
            g.DrawString(abs[abIdx].Name, this.Font, Brushes.White, 5, 5);

            int h = this.Font.Height;
            g.DrawString("Left/Right: Change Algorithm", this.Font, Brushes.Gray, 5, h * 1 + 10);
            g.DrawString("Enter:      Randomize Points", this.Font, Brushes.Gray, 5, h * 2 + 10);
            g.DrawString("Escape:     Close Program", this.Font, Brushes.Gray, 5, h * 3 + 10);
        }

        private void GenerateRandomPoints() {
            points = new PointF[pointsCount];

            Random rnd = new Random();

            double xo = this.DisplayRectangle.Width * 0.1;
            double yo = this.DisplayRectangle.Height * 0.1;
            double w = this.DisplayRectangle.Width * 0.8;
            double h = this.DisplayRectangle.Height * 0.8;

            for(int i = 0; i < pointsCount; i++) {
                float x = (float)(xo + rnd.NextDouble() * w);
                float y = (float)(yo + rnd.NextDouble() * h);
                points[i] = new PointF(x, y);
            }

            //List<PointF> pts = new List<PointF>();
            //w = this.DisplayRectangle.Width / 2;
            //h = this.DisplayRectangle.Height / 2;
            //int r = (int)(Math.Min(w, h) * 0.8);
            //for(int i = 0; i < 360; i += 10) {
            //    float x = (float)(w + r * Math.Cos(i * Math.PI / 180.0));
            //    float y = (float)(h + r * Math.Sin(i * Math.PI / 180.0));
            //    pts.Add(new PointF(x, y));
            //}
            //points = pts.ToArray();
            //pointsCount = pts.Count;

            abs = AlgorithmBase.GetAlgorithms(points);

            RunAlgorithm();
        }
    }
}