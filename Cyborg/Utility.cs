using Rhino.Geometry;
using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyborg
{

    /// <summary>
    /// Contains all generic/geometric operations. 
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        internal static List<double> GetDistanceField(Mesh m, List<Point3d> pts)
        {

            Point3d[] verts = m.Vertices.ToPoint3dArray();
            PointCloud ptsc = new PointCloud(pts);
            double[] vals = new double[verts.Length];

            Parallel.For(0, verts.Length, i =>
            {
                Point3d v = verts[i];
                int id = ptsc.ClosestPoint(v);
                double dist = (v - ptsc[id].Location).Length;
                vals[i] = dist;
            });

            List<double> casted = vals.ToList<double>();
            return casted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        internal static MeshField3d<double> CreateMeshField(Mesh mesh, double[] vals)
        {
            var hem = mesh.ToHeMesh();
            var fn = MeshField3d.Double.Create(hem);
            fn.Set(vals);
            return fn;
        }

        internal static double[] ReparamField(double[] vals)
        {
            double[] newvals = vals;
            double min = vals.Min();
            double max = vals.Max();

            for (int i = 0; i < vals.Length; i++)
            {
                if (vals[i] > 0) newvals[i] = (SpatialSlur.SlurCore.SlurMath.Remap(vals[i], 0, max, 0, 1));
                else newvals[i] = (SpatialSlur.SlurCore.SlurMath.Remap(vals[i], min, 0, -1, 0));
            }

            return newvals;
        }

        /// <summary>
        /// Convert a list of curves to points.
        /// </summary>
        /// <param name="crvs"></param>
        /// <param name="pts"></param>
        /// <param name="res"></param>
        public static void ConvertCrvsToPts(List<Curve> crvs, ref List<Point3d> pts, double res)
        {
            if (crvs.Count == 0) return;
            else
            {
                foreach (Curve c in crvs)
                {
                    double[] t = c.DivideByLength(res, true);
                    foreach (double tt in t)
                    {
                        pts.Add(c.PointAt(tt));
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static Interval GetInterval(List<double> val)
        {
            List<double> tempval = new List<double>(val);
            tempval.Sort();
            var interval = new Interval(tempval[0], tempval[tempval.Count - 1]);
            return interval;
        }

        internal static double FindMax(double a, double b)
        {
            if (a < b) return b;
            else return a;
        }

        internal static double FindMin(double a, double b)
        {
            if (a > b) return b;
            else return a;
        }

        internal static double GetDistanceToCrv(Curve c, Point3d p)
        {
            double t = 0;
            c.ClosestPoint(p, out t);
            double dist = p.DistanceTo(c.PointAt(t));
            return dist;
        }

    }

}
