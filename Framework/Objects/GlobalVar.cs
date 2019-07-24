using System;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class GlobalVar : BaseGameObject
    {
        private string varValue;               
        

        public GlobalVar()
        {
            canHaveRefs = false;
            varValue = "";            
        }

        public void setVarValue(string v)
        {
            varValue = v;
        }

        public float FloatValue
        {
            get {
                try {
                    return float.Parse(varValue);
                } catch {
                    return 0;                    
                }                
            }
            
        }

        public string StringValue
        {
            get { return varValue; }
            
        }

        public override void Dispose()
        {
            
        }

       
    }
}
