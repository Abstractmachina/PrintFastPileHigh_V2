using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;

namespace Cyborg
{
    public class BlendFieldsComponent : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the BlendFieldsComponent class.
        /// </summary>
        public BlendFieldsComponent()
          : base("Blend Fields", "FBlend",
              "Blend two fields at specified parameters t",
              "Cyborg", "Field")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Base Mesh", "M", "Base Mesh.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 0", "F0", "First Field.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Field 1", "F1", "Second Field.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Blend Parameters", "t", "Parameters at which blend is sampled", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddNumberParameter("Blend Values", "V", "blend values as tree. Needs to be passed back into fields.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Blend Fields", "F", "blend fields.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            Mesh mesh = null;
            IField3d<double> f0 = null;
            IField3d<double> f1 = null;
            List<double> t = new List<double>();

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref f0)) return;
            if (!DA.GetData(2, ref f1)) return;
            if (!DA.GetDataList(3, t)) return;

            var hem = mesh.ToHeMesh();

            //List<FuncField3d<double>> blendfields = new List<FuncField3d<double>>();
            List<MeshField3d<double>> blendfields = new List<MeshField3d<double>>();

            //DataTree<double> blends = new DataTree<double>();
            for (int j = 0; j < t.Count; j++)
            {

                GH_Path pth = new GH_Path(j);
                List<double> currentBlend = new List<double>();


                foreach (Point3d p in mesh.Vertices)
                {
                    double val = SpatialSlur.SlurCore.SlurMath.Lerp(f0.ValueAt(p), f1.ValueAt(p), t[j]);
                    currentBlend.Add(val);
                }

                //blends.AppendRange(currentBlend, pth);

                var field = MeshField3d.Double.Create(hem);

                field.Set(currentBlend);
                blendfields.Add(field);
                //blendfields.Add(FuncField3d.Create(i => field.ValueAt(i)));
            }

           
            DA.SetDataList(0, blendfields);
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
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6aba6ae9-faf2-47fc-9869-d34edf2f214c"); }
        }
    }
}