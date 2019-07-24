using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using WeatherGame.Framework.Objects;
using System.Xml.Serialization;
using WeatherGame.Framework.Utilities;

namespace WeatherGame.Framework.World
{

    public static class GameData
    {
        public static void SaveGameData()
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace ws in worldSpaces)
            {               
                string dataFilePath = Utils.GetRootPath() + "\\Content\\export\\World\\" + ws.DataFileName;

                SerializableGamedData w = new SerializableGamedData();
                w.ws = ws;

                Stream stream = File.Open(dataFilePath, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, w);
                stream.Close();                
            }                 
        }

        public static void LoadGameData()
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace ws in worldSpaces)
            {                
                string dataFilePath = Utils.GetRootPath() + "\\Content\\export\\World\\" + ws.DataFileName;
                if (File.Exists(dataFilePath))
                {
                    SerializableGamedData w = new SerializableGamedData();
                    w.ws = ws;
                    Stream stream = File.Open(dataFilePath, FileMode.Open);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    w = (SerializableGamedData)bFormatter.Deserialize(stream);
                    stream.Close();                    
                }
            }                      
        }

        public static void Dispose()
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace ws in worldSpaces)
            {
                ws.Dispose();
            }     
        }

    }

    [Serializable()]
    public class SerializableGamedData : ISerializable
    {
        public WorldSpace ws;

        public SerializableGamedData() { }

        public SerializableGamedData(SerializationInfo info, StreamingContext ctxt)
        {
            string worldObjId = info.GetString("worldObjId");
            ws = WorldData.GetObject(worldObjId) as WorldSpace; 
            foreach (Cell c in ws.AllCells)
            {
                c.CellObjectRefs = (Dictionary<String, GameObjectReference>)info.GetValue(c.BaseObjectId + "-objRefs", typeof(Dictionary<String, GameObjectReference>));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            List<Cell> cells = ws.AllCells;
            info.AddValue("worldObjId", ws.BaseObjectId);
            foreach (Cell c in cells)
            {
                Dictionary<String, GameObjectReference> cellObjectRefs = c.CellObjectRefs;
                info.AddValue(c.BaseObjectId + "-objRefs", cellObjectRefs);
            }
            
        }
    }
}
