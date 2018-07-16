using System;
using System.Collections.Generic;
using Cyborg.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;

namespace Cyborg
{
    public class PlaneTrimComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlaneTrimComponent class.
        /// </summary>
        public PlaneTrimComponent()
          : base("Plane Trim", "PTrim",
              "Trim field with plane.",
              "Cyborg", "Operations")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "M", "Base Mesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field", "F", "Field to be trimmed.", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Trim Plane", "P", "Trim Plane.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Trim Value", "V", "Optional value set for all points in trim.", GH_ParamAccess.item, 1.00d);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Trimmed Field", "F", "Trimmed Field.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            IField3d<double> f = null;
            Plane plane = new Plane();
            double val = 1.00d;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref f)) return;
            if (!DA.GetData(2, ref plane)) return;
            if (!DA.GetData(3, ref val)) return;

            var hem = mesh.ToHeMesh();
            var fn = MeshField3d.Double.Create(hem);

            List<double> vals = new List<double>();

            foreach (Point3d p in mesh.Vertices)
            {
                Vector3d vec = plane.Origin - p;
                double proj = vec * plane.Normal; //dot product

                if (proj > 0) vals.Add(val);

                else vals.Add(f.ValueAt(p));
            }
            fn.Set(vals);


            DA.SetData(0, fn);

        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.PTrim;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2aac0dc6-9de9-41e2-a6d7-853fb5ee550b"); }
        }
    }
}