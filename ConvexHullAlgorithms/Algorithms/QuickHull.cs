using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ConvexHullAlgorithms.Algorithms {
    public class QuickHull : AlgorithmBase {
        public QuickHull(PointF[] points) : base(points) { }

        public override PointF[] Run() {
            PointF a = GetLeftMost(points);
            PointF b = GetRightMost(points);

            var s1 = points.Where(p => GetPointLocation(p, a, b) == 1).ToList();
            var s2 = points.Where(p => GetPointLocation(p, b, a) == 1).ToList();

            List<PointF> ch = new List<PointF>();
            ch.AddRange(FindHull(s1, a, b));
            ch.AddRange(FindHull(s2, b, a));

            ch = ch.Distinct().ToList();
            ch.Add(a);
            return ch.ToArray();
        }

        public override IEnumerable<PointF[]> RunYield() {
            PointF a = GetLeftMost(points);
            PointF b = GetRightMost(points);

            var s1 = points.Where(p => GetPointLocation(p, a, b) == 1).ToList();
            var s2 = points.Where(p => GetPointLocation(p, b, a) == 1).ToList();

            List<PointF> l1 = new List<PointF>();
            List<PointF> l2 = new List<PointF>();
            int n = 10;
            int m = Math.Max(s1.Count, s2.Count);
            for(int i = 0; i < m; i += n) {
                l1 = i < s1.Count ? FindHull(s1.Skip(i).Take(n).ToList(), a, b) : FindHull(s1, a, b);
                l2 = i < s2.Count ? FindHull(s2.Skip(i).Take(n).ToList(), b, a) : FindHull(s2, b, a);
                yield return l1.Concat(l2).Distinct().ToArray();
            }

            yield return Run();
        }

        private List<PointF> FindHull(List<PointF> pts, PointF a, PointF b) {
            if(pts.Count == 0) return new List<PointF>() { a, b };

            double maxD = 0;
            int ci = -1;
            for(int i = 0; i < pts.Count; i++) {
                double d = DistancePointToLine(pts[i], a, b);
                if(d > maxD) {
                    maxD = d;
                    ci = i;
                }
            }

            PointF c = pts[ci];
            //var s0 = pts.Where(p => IsPointInsideTriangle(p, a, c, b));
            var s1 = pts.Where(p => GetPointLocation(p, a, c) == 1).ToList();
            var s2 = pts.Where(p => GetPointLocation(p, c, b) == 1).ToList();

            List<PointF> r = new List<PointF>();
            r.AddRange(FindHull(s1, a, c));
            r.AddRange(FindHull(s2, c, b));

            return r;
        }

        // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points
        private double DistancePointToLine(PointF p, PointF a, PointF b) {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            return Math.Abs(dy * p.X - dx * p.Y + b.X * a.Y - b.Y * a.X) / Math.Sqrt(dx * dx + dy * dy);
        }

        // https://www.geeksforgeeks.org/check-whether-a-given-point-lies-inside-a-triangle-or-not/
        //private bool IsPointInsideTriangle(PointF p, PointF a, PointF b, PointF c) {
        //    double a1 = AreaTriangle(a, b, c);
        //    double a2 = AreaTriangle(p, a, b);
        //    double a3 = AreaTriangle(p, b, c);
        //    double a4 = AreaTriangle(p, a, c);
        //    return a1 == a2 + a3 + a4;
        //}

        //private double AreaTriangle(PointF a, PointF b, PointF c) {
        //    return Math.Abs((a.X * (b.Y - c.Y) +
        //                     b.X * (c.Y - a.Y) +
        //                     c.X * (a.Y - b.Y)) / 2.0);
        //}
    }
}