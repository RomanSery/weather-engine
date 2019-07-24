using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using WeatherGame.Framework.Rendering;
using System.Runtime.Serialization;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.World;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Utilities;

namespace WeatherGame.Framework.Scripting
{
    public static class ScriptManager
    {
        private static CodeDomProvider provider = null;

        public static string scriptWrapper =
        @"
        using System;
        using System.Collections.Generic;
        using System.Text;
        using WeatherGame.Framework.Interfaces;        
        using WeatherGame.RenderLoop;
        using SlimDX;
        using SlimDX.DXGI;
        using SlimDX.Direct3D10;
        using D3D10 = SlimDX.Direct3D10;
        using WeatherGame.Framework;
        using WeatherGame.Framework.Input;
        using WeatherGame.Framework.Utilities;
        using WeatherGame.Framework.Rendering;
        using WeatherGame.Framework.Player;
        using WeatherGame.Framework.Interfaces;
        using WeatherGame.Framework.Objects;

        namespace WeatherGame.Framework.Scripting.Scripts
        {
                public class Script_{CLASS_NAME} : Script
                {
                        {SCRIPT}
                }
        }
        ";

        public static string scriptTemplate =
@"
public override void Execute(GameObjectReference objRef)
{
                            
}
public override void AssignScript(GameObjectReference objRef)
{
                            
}
public override void UnAssignScript(GameObjectReference objRef)
{
                            
}
";

        public static void Initialize()
        {
            provider = CodeDomProvider.CreateProvider("CSharp");


            List<BaseGameObject> scripts = WorldData.GetObjectsByType(typeof(Script));
            foreach (Script s in scripts)
            {
                string errMsg = string.Empty;
                Type className = null;
                CompileScript(s.ScriptText, s.BaseObjectId, out errMsg, false, out className);
                if (errMsg.Length > 0) Console.WriteLine("Script " + s.BaseObjectId + ": " + errMsg);
                s.ClassName = className;
            }

            UpdateAllObjectsAfterCompile(null);
        }

        public static bool CompileScript(string scriptSource, string scriptName, out string errMsg, bool updateAllObjects, out Type className)
        {            
            string binaryPath = Utils.GetRootPath() + "\\Content\\Scripts\\bin\\";
            CompilerParameters cp = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                TreatWarningsAsErrors = false,
            };


            // Add references to all the assemblies we might need.
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            cp.ReferencedAssemblies.Add(executingAssembly.Location);
            foreach (AssemblyName assemblyName in executingAssembly.GetReferencedAssemblies())
                cp.ReferencedAssemblies.Add(Assembly.Load(assemblyName).Location);

            string source = scriptWrapper.Replace("{SCRIPT}", scriptSource).Replace("{CLASS_NAME}", scriptName);
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, source);
            
            if (cr.Errors.Count > 0)
            {
                errMsg = "";
                className = null;
                foreach (CompilerError ce in cr.Errors)
                {
                    errMsg += ce.ToString() + "\n";                    
                }
                return false;
            }
            else
            {
                Assembly a = cr.CompiledAssembly;
                File.Copy(cp.OutputAssembly, binaryPath + scriptName + ".dll", true);                               
                
                if (updateAllObjects)
                {
                    UpdateAllObjectsAfterCompile(scriptName);
                }

                errMsg = "";
                className = a.GetTypes()[0];
                return true;
            }           
        }

        private static void UpdateAllObjectsAfterCompile(string scriptId)
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace ws in worldSpaces)
            {
                List<Cell> cells = ws.AllCells;
                foreach (Cell c in cells)
                {
                    Dictionary<String, GameObjectReference> objRefs = ws.GetAllObjRefs(c);
                    foreach (GameObjectReference objRef in objRefs.Values)
                    {
                        if (string.IsNullOrEmpty(objRef.UpdateScriptId)) continue;
                        if (scriptId != null && objRef.UpdateScriptId != scriptId) continue;
                        
                        if (objRef.updateScript != null) objRef.updateScript.UnAssignScript(objRef);  
                      
                        Script newScript = WorldData.GetObject(objRef.UpdateScriptId) as Script;
                        objRef.updateScript = (Script)Activator.CreateInstance(newScript.ClassName);
                        objRef.updateScript.AssignScript(objRef);
                        objRef.UpdateScriptId = newScript.BaseObjectId;                        
                    }
                }
            }
        }
       

        public static void SetExecScripts(bool exec)
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace ws in worldSpaces)
            {
                List<Cell> cells = ws.AllCells;
                foreach (Cell c in cells)
                {
                    Dictionary<String, GameObjectReference> objRefs = ws.GetAllObjRefs(c);
                    foreach (GameObjectReference objRef in objRefs.Values)
                    {
                        objRef.enableScripts = exec;
                        if (exec == false && objRef.updateScript != null) objRef.updateScript.UnAssignScript(objRef);
                    }
                }
            }
        }
       
    }
}
