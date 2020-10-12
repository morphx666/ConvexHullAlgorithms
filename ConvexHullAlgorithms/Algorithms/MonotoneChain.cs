using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvexHullAlgorithms.Algorithms {
    public class MonotoneChain : AlgorithmBase {
        public MonotoneChain(PointF[] points) : base(points) { }

        public override PointF[] Run() {
            PointF[] r = new PointF[points.Length];
            Array.Copy(points, r, points.Length);
            Array.Sort(r, (p1, p2) => Math.Sign(p1.X == p2.X ? p1.Y - p2.Y : p1.X - p2.X));

            Stack<PointF> lower = new Stack<PointF>();
            Stack<PointF> upper = new Stack<PointF>();
            for(int i = 0; i < r.Length; i++) {
                while(lower.Count > 1 && Cross(lower.ElementAt(1), lower.Peek(), r[i]) <= 0) lower.Pop();
                lower.Push(r[i]);

                int j = r.Length - i - 1;
                if(j >= 0) {
                    while(upper.Count > 1 && Cross(upper.ElementAt(1), upper.Peek(), r[j]) <= 0) upper.Pop();
                    upper.Push(r[j]);
                }
            }

            //lower.Pop();
            upper.Pop();

            return lower.Concat(upper).ToArray();
        }

        public override IEnumerable<PointF[]> RunYield() {
            PointF[] r = new PointF[points.Length];
            Array.Copy(points, r, points.Length);
            Array.Sort(r, (p1, p2) => Math.Sign(p1.X == p2.X ? p1.Y - p2.Y : p1.X - p2.X));

            Stack<PointF> lower = new Stack<PointF>();
            Stack<PointF> upper = new Stack<PointF>();
            for(int i = 0; i < r.Length; i++) {
                while(lower.Count > 1 && Cross(lower.ElementAt(1), lower.Peek(), r[i]) <= 0) lower.Pop();
                lower.Push(r[i]);

                int j = r.Length - i - 1;
                if(j >= 0) {
                    while(upper.Count > 1 && Cross(upper.ElementAt(1), upper.Peek(), r[j]) <= 0) upper.Pop();
                    upper.Push(r[j]);
                }

                yield return lower.Concat(upper).ToArray();
            }

            //lower.Pop();
            upper.Pop();

            yield return lower.Concat(upper).ToArray();
        }
    }
}
