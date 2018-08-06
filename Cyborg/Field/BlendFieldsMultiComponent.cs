using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Cyborg.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using SpatialSlur.SlurCore;
using SpatialSlur.SlurField;
using SpatialSlur.SlurRhino;

namespace Cyborg
{
    public class BlendFieldsComponentMulti : GH_Component
    {

        List<string> debugLog = new List<string>();

        /// <summary>
        /// Initializes a new instance of the BlendFieldsComponent class.
        /// </summary>
        public BlendFieldsComponentMulti()
          : base("Blend Fields Multi-threaded", "FBlendMult",
              "Blend two fields at specified parameters t - multi threaded",
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
            pManager.AddTextParameter("debug", "D", ".", GH_ParamAccess.list);
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

            List<MeshField3d<double>> blendfields = new List<MeshField3d<double>>();

            for (int j = 0; j < t.Count; j++)
            {
                Point3d[] pts = mesh.Vertices.ToPoint3dArray();
                double[] results = new double[pts.Length];

                debugLog.Add(pts.Length.ToString());

                Parallel.For(0, pts.Length, i =>
                {
                    results[i] = SpatialSlur.SlurCore.SlurMath.Lerp(f0.ValueAt(pts[i]), f1.ValueAt(pts[i]), t[j]);
                });

                var field = Utility.CreateMeshField(mesh, results);

                blendfields.Add(field);

            }

            DA.SetDataList(0, debugLog);
            DA.SetDataList(1, blendfields);

        }

        private void test(Mesh m)
        {

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
                return Resources.blendFields;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6aba6ae9-faf2-47fc-9869-d34edf2f214d"); }
        }


        /*
        public void ParallelEvaluateExample<T>(IField3d<T> field, Vec3d[] points, T[] result, bool parallel)
        {
            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, points.Length), range => LoopBody(range.Item1, range.Item2));
            else
                LoopBody(0, points.Length);

            void LoopBody(int from, int to)
            {
                for(int i = from; i < to; i++)
                    result[i] = field.ValueAt(points[i]);
            }
        }
        */
    }
}

