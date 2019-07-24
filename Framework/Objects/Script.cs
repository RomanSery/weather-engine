using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class Script : BaseGameObject
    {
        private string scriptText;

        private string argsDesc;        

        [NonSerialized]
        private Type className;        

        public Script() : base()
        {
            canHaveRefs = false;
            scriptText = "";
            argsDesc = "";
        }

        public void setScriptText(string v)
        {
            scriptText = v;
        }

        public string ScriptText
        {
            get { return scriptText; }
        }

        public Type ClassName
        {
            get { return className; }
            set { className = value; }
        }        

        public string ScriptArgsDesc
        {
            get { return argsDesc; }
            set { argsDesc = value; }
        }

        public virtual void AssignScript(GameObjectReference objRef) { }
        public virtual void UnAssignScript(GameObjectReference objRef) { }
        public virtual void Execute(GameObjectReference objRef) { }

      
        public override void Dispose()
        {
            
        }

       
    }
}
