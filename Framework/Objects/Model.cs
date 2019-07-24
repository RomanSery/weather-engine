using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using SlimDX.Direct3D10;
using WeatherGame.RenderLoop;
using WeatherGame.Framework.Mesh3d;
using System.Reflection;
using SlimDX;
using System.Xml.Serialization;
using WeatherGame.Framework.Utilities;



namespace WeatherGame.Framework.Objects
{
    [Serializable]    
    public class Model : BaseGameObject
    {
        [NonSerialized()]        
        private Mesh3D mesh3d;       
        
        private string fileName;        
        private string inputLayout;

        //physics
        private bool staticMesh;        
        private float mass;

        public Model()
        {
            mesh3d = null;
            canHaveRefs = true;
        
            staticMesh = true;
            mass = 10;
        }
                
        public Mesh MeshObj
        {
            get 
            {
                if (mesh3d == null)
                {
                    try
                    {
                        initMesh3d(fileName, inputLayout);
                    }
                    catch { }                    
                }
                return mesh3d == null ? null : mesh3d.meshObj; 
            }
        }
        
        public Mesh3D Mesh3d
        {
            get 
            {
                if (mesh3d == null)
                {
                    try
                    {
                        initMesh3d(fileName, inputLayout);
                    }
                    catch { }
                }
                return mesh3d; 
            }
        }

        public string FileName { get { return fileName; } set { fileName = value;  } }
        public string Layout { get { return inputLayout; } set { inputLayout = value; } }


        #region Physics
        public bool StaticMesh
        {
            get { return staticMesh; }
            set { staticMesh = value; }
        }
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }              
        #endregion


        public override void Dispose()
        {
            if (mesh3d != null)
                mesh3d.Dispose();
        }

        private void initMesh3d(string file, string layout)
        {
            string s = Utils.GetRootPath() + "\\Content\\export\\" + file;

            if (File.Exists(s) == false) return;

            FieldInfo myf = typeof(MeshInputElements10).GetField(layout);
            InputElement[] ie = (InputElement[])myf.GetValue(null);

            Stream stream = File.Open(s, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            object[] context = new object[] { Game.Device, ie };
            bFormatter.Context = new StreamingContext(StreamingContextStates.All, context);
            mesh3d = (Mesh3D)bFormatter.Deserialize(stream);
            stream.Close();

            mesh3d.inputElements = ie;
            mesh3d.meshFileName = file;            

            mesh3d.ComputeBoundingBox();            
        }
        public void UpdateMesh()
        {
            initMesh3d(mesh3d.meshFileName, inputLayout);            
        }
       
    }
}
