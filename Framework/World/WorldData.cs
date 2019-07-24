using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using WeatherGame.Framework.Objects;
using System.Xml.Serialization;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Mesh3d;
using SlimDX.Direct3D10;

namespace WeatherGame.Framework.World
{

    public static class WorldData
    {
        private static Dictionary<String, BaseGameObject> data;       
        

        public static bool AddObject(BaseGameObject obj)
        {
            if (data == null) LoadData();

            if (obj == null || string.IsNullOrEmpty(obj.BaseObjectId) || string.IsNullOrEmpty(obj.BaseObjectName)) return false;

            if (data.ContainsKey(obj.BaseObjectId)) return false;

            data.Add(obj.BaseObjectId, obj);
            return true;
        }
        public static bool UpdateObject(BaseGameObject obj)
        {
            if (data == null) LoadData();

            if (obj == null || string.IsNullOrEmpty(obj.BaseObjectId) || string.IsNullOrEmpty(obj.BaseObjectName)) return false;

            if (!data.ContainsKey(obj.BaseObjectId)) return false;

            data[obj.BaseObjectId] = obj;            
            return true;
        }

        public static bool DeleteObject(BaseGameObject obj)
        {
            if (data == null) LoadData();

            if (obj == null || string.IsNullOrEmpty(obj.BaseObjectId) || string.IsNullOrEmpty(obj.BaseObjectName)) return false;

            if (!data.ContainsKey(obj.BaseObjectId)) return false;

            
            if (obj.CanHaveRefs)
            {
                //remove it from all cells
                List<BaseGameObject> allCells = WorldData.GetObjectsByType(typeof(Cell));
                foreach (Cell c in allCells) c.DeleteAllObjRefByBaseObjId(obj.BaseObjectId);
            }

            return data.Remove(obj.BaseObjectId);                        
        }

        public static BaseGameObject GetObject(string id)
        {            
            if (data == null) LoadData();

            if (string.IsNullOrEmpty(id)) return null;
            return data.ContainsKey(id) ? data[id] : null;            
        }        

        public static List<BaseGameObject> GetObjectsByType(Type t)
        {
            if (data == null) LoadData();

            List<BaseGameObject> arr = new List<BaseGameObject>();
            foreach (KeyValuePair<String, BaseGameObject> entry in data)
            {
                if (!string.IsNullOrEmpty(entry.Key) && entry.Value.GetType() == t)
                    arr.Add(entry.Value);                
            }

            return arr;            
        }

        public static void SaveData()
        {            
            string s = Utils.GetRootPath() + "\\Content\\export\\World\\world.data";
            
            SerializableWorldData w = new SerializableWorldData();
            w.data = data;
            
            Stream stream = File.Open(s, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();            
            bFormatter.Serialize(stream, w);
            stream.Close();
        }

        private static void LoadData()
        {
            string s = Utils.GetRootPath() + "\\Content\\export\\World\\world.data";
            if (!File.Exists(s))
            {
                data = new Dictionary<String, BaseGameObject>();
                return;
            }
                        
            SerializableWorldData w;
            Stream stream = File.Open(s, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            w = (SerializableWorldData)bFormatter.Deserialize(stream);
            stream.Close();

            data = w.data;            
        }

        public static void Dispose()
        {
            if (data == null) return;
            
            foreach (KeyValuePair<String, BaseGameObject> entry in data)
            {
                entry.Value.Dispose();
            }            
        }

    }

    [Serializable()]
    public class SerializableWorldData : ISerializable
    {
        public Dictionary<String, BaseGameObject> data;        

        public SerializableWorldData() { }

        public SerializableWorldData(SerializationInfo info, StreamingContext ctxt)
        {         
            data = (Dictionary<String, BaseGameObject>)info.GetValue("data", typeof(Dictionary<String, BaseGameObject>));            
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {            
            info.AddValue("data", data);            
        }
    }
}
