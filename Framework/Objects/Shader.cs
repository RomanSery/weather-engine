using System;
using System.Collections.Generic;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D10;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.Rendering;
using WeatherGame.RenderLoop;
using D3D10 = SlimDX.Direct3D10;
using System.IO;
using System.Xml.Serialization;
using WeatherGame.Framework.Utilities;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class Shader : BaseGameObject   
    {
        private string shaderString;

        [NonSerialized]        
        private Effect effect;
        [NonSerialized]        
        private Dictionary<string, EffectVariable> vars;

        public Shader()
        {
            canHaveRefs = false;
            effect = null;
            vars = new Dictionary<string, EffectVariable>();
            shaderString = null;
        }

        public string Recompile()
        {
            try
            {
                EffectIncludeHelper h = new EffectIncludeHelper();
                ShaderFlags f = ShaderFlags.Debug | ShaderFlags.EnableBackwardsCompatibility;
                if (BaseObjectId == "rain.fx" || BaseObjectId == "halo.fx" || BaseObjectId == "snow.fx" || BaseObjectId == "debugger.fx")
                    f = ShaderFlags.Debug;

                effect = Effect.FromString(Game.Device, shaderString, "fx_4_0", f, EffectFlags.None, null, h, null);
                vars = new Dictionary<string, EffectVariable>(effect.Description.GlobalVariableCount);

                for (int i = 0; i < effect.Description.GlobalVariableCount; i++)
                {
                    EffectVariable v = effect.GetVariableByIndex(i);
                    vars.Add(v.Description.Name, v);
                }
                return "";
            }
            catch (Exception e)
            {
                effect = null;
                return e.Message;                
            }
        }

        public Effect EffectObj
        {
            get
            {
                //if (effect == null) Recompile();                
                return effect;
            }
        }

        public string ShaderString
        {
            get { return shaderString; }
            set { shaderString = value; }
        }
        public Dictionary<string, EffectVariable> Vars
        {
            get 
            {
                //if (vars == null) Recompile();   
                return vars; 
            }
        }
        public EffectVariable GetVar(string name)
        {
            //if (vars == null) Recompile();   

            if (vars != null && vars.ContainsKey(name))
                return vars[name];
            return null;                      
        }

        public override void Dispose()
        {
            if(effect != null && !effect.Disposed)
                effect.Dispose();            
        }
    }

    internal class EffectIncludeHelper : Include
    {
        public void Close(Stream stream)
        {
            stream.Close();
        }

        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            string s = Utils.GetRootPath() + "\\Content\\Shaders\\" + fileName;
            using (StreamReader sr = new StreamReader(s))
            {
                string str = sr.ReadToEnd();
                stream = new MemoryStream(System.Text.Encoding.Default.GetBytes(str));
            }
        }
    }


    public static class ShaderHelper
    {
        public const string DiffTex = "diffTexture";
        public const string WorldMatrix = "World";
        public const string ViewMatrix = "View";
        public const string ProjectionMatrix = "Projection";
        public const string ViewProj = "ViewProj";
        public const string CameraPosition = "cameraPosition";
        public const string WVP = "WorldViewProj";
        public const string WorldView = "WorldView";
        
        static Dictionary<string, InputLayout> inputLayoutCache;        

        public static InputLayout ConstructInputLayout(D3D10.InputElement[] inputElements, ShaderSignature ss)
        {
            if(inputLayoutCache == null)
                inputLayoutCache = new Dictionary<string, InputLayout>();

            string key = inputElements.GetHashCode().ToString() + "_" + ss.GetHashCode().ToString();
            if (inputLayoutCache.ContainsKey(key))
                return inputLayoutCache[key];
            else
            {
                InputLayout l = new InputLayout(Game.Device, inputElements, ss);                
                inputLayoutCache.Add(key, l);
                return l;
            }

        }

        public static void UpdateCommonEffectVars(Shader effect, Matrix world)
        {
            if (effect == null) return;
            Dictionary<string, EffectVariable> vars = effect.Vars;
            if (vars == null) return;

            foreach (KeyValuePair<string, EffectVariable> pair in vars)
            {
                EffectVariable v = pair.Value;

                if (pair.Key == ViewMatrix)
                    v.AsMatrix().SetMatrix(Camera.ViewMatrix);
                else if (pair.Key == ProjectionMatrix)
                    v.AsMatrix().SetMatrix(Camera.ProjectionMatrix);
                else if (pair.Key == WorldMatrix)
                    v.AsMatrix().SetMatrix(world);
                else if (pair.Key == WVP)
                    v.AsMatrix().SetMatrix(world * Camera.ViewMatrix * Camera.ProjectionMatrix);
                else if (pair.Key == WorldView)
                    v.AsMatrix().SetMatrix(world * Camera.ViewMatrix);
                else if (pair.Key == ViewProj)
                    v.AsMatrix().SetMatrix(Camera.ProjectionMatrix * Camera.ViewMatrix);
                else if (pair.Key == CameraPosition)
                    v.AsVector().Set(new Vector4(Camera.Position, 1));
            }
        }

        public static void Dispose()
        {
            foreach (KeyValuePair<string, InputLayout> entry in inputLayoutCache)
            {
                if(entry.Value != null && !entry.Value.Disposed)
                    entry.Value.Dispose();
            }  
        }


    }

   
}
