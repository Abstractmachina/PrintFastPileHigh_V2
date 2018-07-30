using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cyborg.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;

namespace Cyborg.Field
{

    
    public class AreaMeshFieldComponent : GH_Component
    {
        List<string> debug = new List<string>();
        /// <summary>
        /// Initializes a new instance of the MeshFieldComponent class.
        /// </summary>
        public AreaMeshFieldComponent()
          : base("Area Mesh Field", "AMField",
              "Create a field from closed boundary curves on a base mesh.",
              "Cyborg", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "M", "Base mesh of the field.", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Drawing Input", "IN", "Closed boundary curves that describe field.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Iso Value", "IV", "Zero Iso Value Adjustment", GH_ParamAccess.item);
            pManager.AddNumberParameter("Tolerance", "T", "0-iso tolerance", GH_ParamAccess.item, 1d);


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("Mesh Field", "F", "Mesh Field Result", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "D", ".", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            debug.Add("DEBUG");
            Mesh m = null;
            double iso = 0d;
            double tolerance = 1d;

            List<Curve> crvs = new List<Curve>();

            if (!DA.GetData(0, ref m)) return;
            if (!DA.GetDataList<Curve>(1, crvs)) return;
            if (!DA.GetData(2, ref iso)) return;
            if (!DA.GetData(3, ref tolerance)) return;

            iso = Math.Abs(iso);

            Point3d[] pts = m.Vertices.ToPoint3dArray();
            
            double[] vals = CalcField(crvs, pts);

            vals = RemapField(vals);

            var hem = m.ToHeMesh();
            var field = MeshField3d.Double.Create(hem);
            field.Set(vals);

            DA.SetData(0, field);
            DA.SetDataList(1, debug);
        }

        /// <summary>
        /// Calculate field values which can be mapped onto input mesh.
        /// </summary>
        /// <param name="crvs">Input boundary curves.</param>
        /// <param name="pts">mesh vertices.</param>
        /// <returns></returns>
        private double[] CalcField(List<Curve> crvs, Point3d[] pts)
        {
            double[] vals = new double[pts.Length];
            Parallel.For(0, pts.Length, i =>
            {
                Point3d p = pts[i];
                bool inside = false;
                int count = 0;
                double closestDist = 1000000d;

                while (inside == false && count < crvs.Count)
                {
                    Curve c = crvs[count];

                    double dist = GetDistanceToCrv(c, p);

                    if (c.Contains(p) == PointContainment.Inside)
                    {
                        vals[i] = dist * -1;
                        inside = true;

                    }
                    else
                    {
                        closestDist = FindMin(closestDist, dist);
                    }
                    count++;
                }

                if (vals[i] >= 0) vals[i] = closestDist;
            });

            return vals;
        }

        private double[] RemapField(double[] vals)
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

        private double FindMax(double a, double b)
        {
            if (a > b) return a;
            if (a < b) return b;
            else return a;
        }

        private double FindMin(double a, double b)
        {
            if (a < b) return a;
            if (a > b) return b;
            else return a;
        }

        private double GetDistanceToCrv(Curve c, Point3d p)
        {
            double t = 0;
            c.ClosestPoint(p, out t);
            double dist = p.DistanceTo(c.PointAt(t));
            return dist;
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
                return Resources.AMField;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6e148e97-94bc-4df0-a5fc-60c565add81d"); }
        }
    }
}