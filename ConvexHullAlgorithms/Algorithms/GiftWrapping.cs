using System;
using System.Collections.Generic;
using System.Drawing;

namespace ConvexHullAlgorithms.Algorithms {
    public class GiftWrapping : AlgorithmBase {
        public GiftWrapping(PointF[] points) : base(points) { }

        public override PointF[] Run() {
            PointF pointOnHull = GetLeftMost(points);
            PointF[] p = new PointF[points.Length];
            PointF endPoint;
            int i = 0;
            do {
                p[i] = pointOnHull;
                endPoint = points[0];
                for(int j = 0; j < points.Length; j++) {
                    if((endPoint == pointOnHull) ||
                        (GetPointLocation(points[j], p[i], endPoint) == -1))
                        endPoint = points[j];
                }
                i++;
                pointOnHull = endPoint;
            } while(endPoint != p[0]);

            p[i++] = p[0];
            PointF[] r = new PointF[i];
            Array.Copy(p, r, i);
            return r;
        }

        public override IEnumerable<PointF[]> RunYield() {
            PointF pointOnHull = GetLeftMost(points);
            PointF[] p = new PointF[points.Length + 1];
            PointF[] r;
            PointF endPoint;
            int i = 0;
            do {
                p[i] = pointOnHull;
                endPoint = points[0];

                r = new PointF[i + 1];
                Array.Copy(p, r, i);

                for(int j = 0; j < points.Length; j++)
                    if((endPoint == pointOnHull) || (GetPointLocation(points[j], pointOnHull, endPoint) == -1)) {
                        endPoint = points[j];

                        r[i] = endPoint;
                        yield return r;
                    }
                i++;
                pointOnHull = endPoint;

            } while(endPoint != p[0]);

            p[i++] = p[0];
            r = new PointF[i];
            Array.Copy(p, r, i);
            yield return r;
        }
    }
}