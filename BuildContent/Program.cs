using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using SlimDX;
using DXGI = SlimDX.DXGI;
using SlimDX.Direct3D10;
using D3D10 = SlimDX.Direct3D10;
using WeatherGame.Framework;
using System.Reflection;
using BuildTextures;
using Assimp;
using Assimp.Configs;
using WeatherGame.Framework.Mesh3d;

namespace WeatherGame.BuildContent
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (D3D10.Device device = new D3D10.Device(D3D10.DeviceCreationFlags.None))
            {
                //build single mesh from world editor
                string meshArgs = args != null && args.Length > 0 ? args[0] : "skull.X,NormalMesh";//"block1.X,NormalMesh";
                BuildMesh(device, meshArgs);                
            }
            return;            
        }
        


        private static void BuildMesh(D3D10.Device d, string meshArgs)
        {
            string name = meshArgs.Split(',')[0];
            InputElement[] layout =  (InputElement[])typeof(MeshInputElements10).GetField(meshArgs.Split(',')[1]).GetValue(typeof(MeshInputElements10));


            string fileName = ConfigurationSettings.AppSettings["ExportsFolder"] + name;

            AssimpImporter importer = new AssimpImporter();
           LogStream logstream = new LogStream(delegate(String msg, String userData) {
                Console.WriteLine(msg);
            });

            importer.AttachLogStream(logstream);

            importer.Scale = 0;
            importer.XAxisRotation = 0;
            importer.YAxisRotation = 0;
            importer.ZAxisRotation = 0;
            importer.VerboseLoggingEnabled = true;
            importer.RemoveConfigs();
            
            Scene model = importer.ImportFile(fileName,                
                PostProcessSteps.CalculateTangentSpace // calculate tangents and bitangents if possible
            | PostProcessSteps.JoinIdenticalVertices // join identical vertices/ optimize indexing CAUSES A PROBLEM
            //| PostProcessSteps.ValidateDataStructure // perform a full validation of the loader's output
            | PostProcessSteps.ImproveCacheLocality // improve the cache locality of the output vertices
            | PostProcessSteps.RemoveRedundantMaterials // remove redundant materials
            //| PostProcessSteps.FindDegenerates // remove degenerated polygons from the import CAUSES A PROBLEM
            //| PostProcessSteps.FindInvalidData // detect invalid model data, such as invalid normal vectors
            | PostProcessSteps.GenerateUVCoords // convert spherical, cylindrical, box and planar mapping to proper UVs
            | PostProcessSteps.TransformUVCoords // preprocess UV transformations (scaling, translation ...)
            //| PostProcessSteps.FindInstances // search for instanced meshes and remove them by references to one master
            //| PostProcessSteps.LimitBoneWeights // limit bone weights to 4 per vertex
            | PostProcessSteps.OptimizeMeshes // join small meshes, if possible;
            | PostProcessSteps.GenerateSmoothNormals // generate smooth normal vectors if not existing            
            | PostProcessSteps.Triangulate // triangulate polygons with more than 3 edges
            | PostProcessSteps.SortByPrimitiveType // make 'clean' meshes which consist of a single typ of primitives            
            | PostProcessSteps.FlipUVs  // common DirectX issue (Xna also)
            | PostProcessSteps.FixInFacingNormals
            | PostProcessSteps.MakeLeftHanded
            | PostProcessSteps.FlipWindingOrder                        
            );


            MeshHelper.BuildMeshTextures(d, model);






            Mesh3D m = MeshHelper.LoadFromFile(d, model, layout);

            importer.Dispose();            


            Stream stream = File.Open(ConfigurationSettings.AppSettings["ExportsFolder"] + name.Replace(".X", ".mesh"), FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            if (m != null) bFormatter.Serialize(stream, m);
            stream.Close();
        }
    }
}
