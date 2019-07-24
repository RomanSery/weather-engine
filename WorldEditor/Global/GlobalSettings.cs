using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using WeatherGame.Framework;
using WeatherGame.Framework.World;
using WeatherGame.Framework.Objects;

namespace WorldEditor.Global
{
    public static class GlobalSettings
    {        
        public static bool RenderLights = true;
        public static bool RenderVolumes = true;
        public static bool RenderGrid = true;
        public static bool ExecuteScripts = false;

        public static Cell CurrentCell = null;
        public static GameObjectReference CurrentObjRef = null;


        public static Vector4 GridColor = new Vector4(1, 1, 1, 1);
    }
}
