using System;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public abstract class BaseGameObject
    {
        private string baseObjectId = null;
        private string baseObjectName = null;

        protected bool canHaveRefs;
        private string category = null; //architecture, roads, bridghes. interior lights, exterior lights

        public string BaseObjectId
        {
            get { return baseObjectId; }
            set { baseObjectId = value; }
        }

        public string BaseObjectName
        {
            get { return baseObjectName; }
            set { baseObjectName = value; }
        }

        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        public bool CanHaveRefs
        {
            get { return canHaveRefs; }            
        }


        public virtual void Update()
        {

        }

        public abstract void Dispose();        

    }
}
