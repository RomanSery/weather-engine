using System;
using System.Collections.Generic;

using SlimDX;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class WorldSpace : BaseGameObject
    {       
        public const int cellSize = 500;
        private string dataFileName = null;        

        public WorldSpace()
        {
            canHaveRefs = false;            
        }

        public void Initialize()
        {
            
        }
        public override void Dispose()
        {           
            List<BaseGameObject> cells = WorldData.GetObjectsByType(typeof(Cell));
            foreach (Cell c in cells)
            {
                c.Dispose();
            }               
                        
        }


        public void Update()
        {           
            List<BaseGameObject> cells = WorldData.GetObjectsByType(typeof(Cell));
            foreach (Cell c in cells)
            {                
                c.Update();
            }                                      
        }        
       
        public List<Cell> AllCells 
        { 
            get 
            {
                List<Cell> cells = new List<Cell>();

                List<BaseGameObject> allCells = WorldData.GetObjectsByType(typeof(Cell));
                foreach (Cell c in allCells)
                {
                    if (c.WorldSpaceId == this.BaseObjectId) cells.Add(c);
                }

                return cells; 
            } 
        }
        public List<Cell> CellsToDraw
        {
            get
            {
                List<Cell> cells = new List<Cell>();

                List<BaseGameObject> allCells = WorldData.GetObjectsByType(typeof(Cell));
                foreach (Cell c in allCells)
                {
                    if (c.WorldSpaceId == this.BaseObjectId) cells.Add(c);
                }

                return cells; 
            }
        }




        public Dictionary<String, GameObjectReference> GetObjRefsToDraw(Cell c) 
        {
            if (c == null) return new Dictionary<String, GameObjectReference>();

            Dictionary<String, GameObjectReference> models = new Dictionary<String, GameObjectReference>();
            foreach (KeyValuePair<String, GameObjectReference> entry in c.CellObjectRefs)
            {
                if (entry.Value.BaseGameObject is Model) models.Add(entry.Value.RefId, entry.Value);
            }
            return models;            
        }
        public Dictionary<String, GameObjectReference> GetAllModelObjRefs(Cell c) 
        {
            if (c == null) return new Dictionary<String, GameObjectReference>();

            Dictionary<String, GameObjectReference> models = new Dictionary<String, GameObjectReference>();
            foreach (KeyValuePair<String, GameObjectReference> entry in c.CellObjectRefs)
            {
                if (entry.Value.BaseGameObject is Model) models.Add(entry.Value.RefId, entry.Value);
            }
            return models;            
        }
        public Dictionary<String, GameObjectReference> GetAllObjRefs(Cell c)
        {
            if (c == null) return new Dictionary<String, GameObjectReference>();
            return c.CellObjectRefs;
        }




        public Dictionary<String, GameObjectReference> GetLightsToDraw(Cell c)
        {
            if (c == null) return new Dictionary<String, GameObjectReference>();


            Dictionary<String, GameObjectReference> lights = new Dictionary<String, GameObjectReference>();
            foreach (KeyValuePair<String, GameObjectReference> entry in c.CellObjectRefs)
            {
                if (entry.Value.BaseGameObject is Light)
                    lights.Add(entry.Value.RefId, entry.Value);
            }            

            return lights;
        }


        public string DataFileName { get { return dataFileName; } set { dataFileName = value; } }

        //helpers
        public static Vector3 GetRealWorldPos(Vector3 pos, Cell c)
        {   
            if(c == null)
                return new Vector3(pos.X, pos.Y, pos.Z);

            return new Vector3(pos.X + (c.CellPos.X * cellSize), pos.Y, pos.Z + (c.CellPos.Y * cellSize));
            
        }
        public static Matrix GetRealWorldMatrix(Vector3 pos, Cell c)
        {
            if(c == null)
                return Matrix.Translation(pos);

            return Matrix.Translation(pos) * Matrix.Translation(c.CellPos.X * cellSize, 0, c.CellPos.Y * cellSize);
            
        }        
        public static Matrix GetRealWorldMatrixGrid(Vector3 pos, Cell c)
        {
            if (c == null)            
                return Matrix.Translation(pos);            

            return Matrix.Translation(pos) * Matrix.Translation(c.CellPos.X * (cellSize * 2), 0, c.CellPos.Y * (cellSize * 2));            
        }

       

    }


  
}
