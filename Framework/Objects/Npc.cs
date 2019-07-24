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
using WeatherGame.Framework.World;



namespace WeatherGame.Framework.Objects
{
    [Serializable]    
    public class Npc : BaseGameObject
    {
        private string modelId = null;

        private Vector3 facing; //direction NPC is facing
        private float angle; //angle of rotation
        private bool seesPlayer;


        public Npc()
        {           
            canHaveRefs = true;        
        }


        public Model NpcModel { get { return WorldData.GetObject(modelId) as Model; } }
       

        public override void Dispose()
        {
            
        }

       
    }
}
