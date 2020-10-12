using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ConvexHullAlgorithms.Algorithms {
    public class GrahamScan : AlgorithmBase {
        private const double PI2 = 2 * Math.PI;
        public class PointFAngle {
            public PointF Point;
            public double Angle;

            public override string ToString() {
                return $"{Point.X:N2}, {Point.Y:N2} : {Angle:N2}";
            }
        }

        public GrahamScan(PointF[] points) : base(points) { }

        public override PointF[] Run() {
            Stack<PointF> stack = new Stack<PointF>();

            PointF p = GetBottomLeftMost();
            PointF[] sorted = SortPoints(p);

            for(int i = 0; i < sorted.Length; i++) {
                while(stack.Count > 1 && CCW(stack.ElementAt(1), stack.Peek(), sorted[i]) <= 0) stack.Pop();
                stack.Push(sorted[i]);
            }

            stack.Push(p);
            return stack.ToArray();
        }

        public override IEnumerable<PointF[]> RunYield() {
            Stack<PointF> stack = new Stack<PointF>();

            PointF p = GetBottomLeftMost();
            PointF[] sorted = SortPoints(p);

            for(int i = 0; i < sorted.Length; i++) {
                while(stack.Count > 1 && CCW(stack.ElementAt(1), stack.Peek(), sorted[i]) <= 0) stack.Pop();
                stack.Push(sorted[i]);
                yield return stack.ToArray();
            }

            stack.Push(p);
            yield return stack.ToArray();
        }

        private double CCW(PointF p1, PointF p2, PointF p3) {
            PointF v1 = new PointF(p2.X - p1.X, p2.Y - p1.Y);
            PointF v2 = new PointF(p3.X - p2.X, p3.Y - p2.Y);

            return Cross(v2, v1);
        }

        private PointF[] SortPoints(PointF p) {
            List<PointFAngle> s = new List<PointFAngle>();
            for(int i = 0; i < points.Length; i++) {
                s.Add(new PointFAngle() {
                    Point = points[i],
                    Angle = Math.Atan2(points[i].Y - p.Y, points[i].X - p.X)
                });
                if(s[i].Angle < 0) s[i].Angle += PI2;
            }
            s.Sort((p1, p2) => Math.Sign(p1.Angle - p2.Angle));

            bool isDone;
            do {
                isDone = true;
                for(int i = 0; i < s.Count; i++) {
                    for(int j = i + 1; j < s.Count; j++) {
                        if(Math.Abs(s[i].Angle - s[j].Angle) <= 0.000001) {
                            //if(s[i].Angle == s[j].Angle) {
                            double d1 = Distance(s[i].Point, p);
                            double d2 = Distance(s[j].Point, p);

                            if(d1 > d2) s.RemoveAt(j);
                            else s.RemoveAt(i);

                            isDone = false;
                            break;
                        }
                    }
                    if(!isDone) break;
                }
            } while(!isDone);

            PointF[] r = new PointF[s.Count];
            for(int i = 0; i < s.Count; i++) r[i] = s[i].Point;
            return r;
        }

        private PointF GetBottomLeftMost() {
            int[] idxs = new int[points.Length];

            float maxY = points.Max(p => p.Y);
            int idxsCount = 0;

            for(int i = 0; i < points.Length; i++)
                if(points[i].Y == maxY) idxs[idxsCount++] = i;

            PointF[] pts = new PointF[idxsCount];
            for(int i = 0; i < idxsCount; i++) pts[i] = points[idxs[i]];

            return GetLeftMost(pts);
        }
    }
}