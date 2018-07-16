using System;
using System.Collections.Generic;

using Cyborg.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;

namespace Cyborg
{
    public class MeshFieldComponent : GH_Component
    {
        

        /// <summary>
        /// Initializes a new instance of the MeshFieldComponent class.
        /// </summary>
        public MeshFieldComponent()
          : base("Mesh Field", "MField",
              "Create a field from points or curves on a base mesh.",
              "Cyborg", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "M", "Base mesh of the field.", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Drawing Input", "IN", "points or curves that describe field.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Iso Value", "IV", "Zero Iso Value Adjustment", GH_ParamAccess.item);
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
            pManager.AddGenericParameter("Mesh Field", "F", "Mesh Field Result", GH_ParamAccess.item);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            

            Mesh m = null;
            List<GH_GeometricGooWrapper> obj = new List<GH_GeometricGooWrapper>();
            double iso = 0d;

            List<Point3d> pts = new List<Point3d>();
            List<Curve> crvs = new List<Curve>();


            if (!DA.GetData(0, ref m)) return;
            if (!DA.GetDataList<GH_GeometricGooWrapper>(1, obj)) return;
            if (!DA.GetData(2, ref iso)) return;

            var hem = m.ToHeMesh();
            var field = MeshField3d.Double.Create(hem);

            //cast objects to geometry types
            foreach (GH_GeometricGooWrapper g in obj)
            {
                if (g.TypeName == "{Point}")
                {
                    Point3d p = new Point3d();
                    g.CastTo<Point3d>(ref p);
                    pts.Add(p);
                }
                else if (g.TypeName == "{Curve}")
                {
                    Curve c = null;
                    g.CastTo<Curve>(ref c);
                    crvs.Add(c);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input must be points or curves.");
                    return;
                }
            }

            //if curves are inputted, convert them into points
            ConvertCrvsToPts(crvs, ref pts);
            

            //get vector of each field pt to the closest point. 
            List<double> val = GetDistanceField(m, pts);

            Interval interval = GetInterval(val);
            //

            for (int i = 0; i < val.Count; i++)
            {
                val[i] = (SpatialSlur.SlurCore.SlurMath.Remap(val[i], interval.T0, interval.T1, -1, 1)) - iso;
            }

            //

            field.Set(val);

            
            DA.SetData(0, FuncField3d.Create(i => field.ValueAt(i)));
        }



        private static void ConvertCrvsToPts( List<Curve> crvs, ref List<Point3d> pts)
        {
            if (crvs.Count == 0) return;
            else 
            {
                foreach (Curve c in crvs)
                {
                    double[] t = c.DivideByLength(1, true);
                    foreach (double tt in t)
                    {
                        pts.Add(c.PointAt(tt));
                    }
                }
            }
        }

        private List<double> GetDistanceField(Mesh m, List<Point3d> pts)
        {
            List<double> val = new List<double>();
            Point3d[] verts = m.Vertices.ToPoint3dArray();
            PointCloud ptsc = new PointCloud(pts);


            //calc distance field
            foreach (Point3d v in verts)
            {
                int i = ptsc.ClosestPoint(v);
                double dist = (v - ptsc[i].Location).Length;
                val.Add(dist);
            }

            return val;
        }

        private Interval GetInterval(List<double> val)
        {
            List<double> tempval = new List<double>(val);
            tempval.Sort();
            var interval = new Interval(tempval[0], tempval[tempval.Count - 1]);
            return interval;
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.MField;
                //return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9852d8d8-520b-4e1a-a84e-4a5ae8cae88d"); }
        }
    }
}