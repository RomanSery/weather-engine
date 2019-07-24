using System;
using System.Collections.Generic;

using SlimDX;
using WeatherGame.Framework.physics;
using WeatherGame.Framework.World;
using System.Xml.Serialization;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class Cell : BaseGameObject
    {        
        private Vector2 cellPos;        
        private string worldSpaceId;

        [NonSerialized]                
        private Dictionary<String, GameObjectReference> cellObjectRefs = null;
        

        public Cell()
        {
            canHaveRefs = false;
            cellObjectRefs = new Dictionary<String, GameObjectReference>();
            cellPos = new Vector2(0, 0);
            worldSpaceId = null;
        }

        public void Initialize()
        {
            cellObjectRefs = new Dictionary<String, GameObjectReference>();      
        }

        public override void Update()
        {
            if (cellObjectRefs == null) return;
            foreach (KeyValuePair<String, GameObjectReference> entry in cellObjectRefs)
            {
                entry.Value.Update();                
            }            
        }       


        public override void Dispose()
        {
            if (cellObjectRefs == null) return;
            foreach (KeyValuePair<String, GameObjectReference> entry in cellObjectRefs)
            {
                entry.Value.Dispose();
            }            
        }

        public GameObjectReference getObjRefById(string refId)
        {
            if (cellObjectRefs == null) return null;
            if (string.IsNullOrEmpty(refId)) return null;

            return cellObjectRefs.ContainsKey(refId) ? cellObjectRefs[refId] : null;  
        }
        public void UpdateObjRef(GameObjectReference obj)
        {
            if (cellObjectRefs == null) return;

            if (!cellObjectRefs.ContainsKey(obj.BaseObjectId)) return;

            cellObjectRefs[obj.BaseObjectId] = obj;            
        }

        public void AddObjRef(string baseId)
        {
            BaseGameObject baseObj = WorldData.GetObject(baseId);

            GameObjectReference objRef = new GameObjectReference(this.BaseObjectId, baseId);
            
            objRef.RefId = BaseObjectId + "_" + objRef.BaseObjectId + "_" + CellObjectRefs.Count;
            cellObjectRefs.Add(objRef.RefId, objRef);

            if (baseObj is Model)
                PhysicsEngine.AddMeshToSpace(objRef, this);
        }

        public void DeleteAllObjRefByBaseObjId(string baseObjId)
        {
            List<string> removals = new List<string>();
            foreach (KeyValuePair<String, GameObjectReference> entry in CellObjectRefs)
            {
                if (entry.Value.BaseObjectId == baseObjId)
                    removals.Add(entry.Key);                    
            }

            foreach (string key in removals)
            {
                cellObjectRefs.Remove(key);
            }            
        }

        public void DeleteObjRef(string refId)
        {
            if (cellObjectRefs == null || string.IsNullOrEmpty(refId)) return;

            if (!cellObjectRefs.ContainsKey(refId)) return;

            cellObjectRefs.Remove(refId);            
        }

     
        public Vector2 CellPos { get { return cellPos; } set { cellPos = value; } }

        public Dictionary<String, GameObjectReference> CellObjectRefs
        {
            get
            {
                if (cellObjectRefs == null) cellObjectRefs = new Dictionary<String, GameObjectReference>();
                return cellObjectRefs;
            }
            set
            {
                if(value == null)
                    cellObjectRefs = new Dictionary<String, GameObjectReference>();
                else
                    cellObjectRefs = value;
            }
        }

        public string WorldSpaceId { get { return worldSpaceId; } set { worldSpaceId = value; } }

    }
    
   
}
