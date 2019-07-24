using System;
using System.Collections.Generic;
using WeatherGame.Framework.World;
using WeatherGame.Framework.Interfaces;
using SlimDX;
using WeatherGame.Framework.physics;
using WeatherGame.Framework.Mesh3d;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WeatherGame.Framework.Objects
{
    public enum GameObjectRefAttribute
    {
        IncludeInReflectionMap,
        UseReflectionMap,
        RecieveRainSplashes,
        RecieveSnow,
        IsWater,
        IsZeroPlaneReflection,
        UseSpecular
    }


    [Serializable]
    public class GameObjectReference : IPositionable, ISwitchable
    {
        private string baseObjectId = null;
        private string refId = null;
        private string refName = null;
        private string cellContainerId;

        private bool isOn;

        private Dictionary<GameObjectRefAttribute, bool> attributes;

        //3d data
        private Vector3 pos;
        private Matrix rotation;
        private Vector3 rotationValues;
        private Vector3 scale;
        [NonSerialized]        
        private AnimationController animController = null;

        //extra data for lighting
        private Vector3 color;
        private float intensity;
        private float maxRange;        
        private Vector3 target; //for spot light
        private Vector3 endPoint; //for area light
        private float _lerpInc;
        private float innerAngle;	// Full angle RADIANS -- not the half angle.  Remember to mult by 0.5 before doing cone calcs in shader
        private float outerAngle;	// Full angle RADIANS -- not the half angle.  Remember to mult by 0.5 before doing cone calcs in shader	
        private float boxHeight; //for box light
        private float boxWidth; //for box light
        private float boxDepth; //for box light


        //materials
        private string[] materialIDs = null;
        private int? selectedSubset = null;

        //physics
        private Matrix entityTransform;
        [NonSerialized]        
        private BEPUphysics.ISpaceObject physicsObj;

        //Scripts
        [NonSerialized]
        public bool enableScripts = false;
        private string updateScriptId = null;
        private string[] updateScriptArgs = null;
        [NonSerialized]
        public Script updateScript = null;

        //water
        private string waterSettingId;
        [NonSerialized]  
        public float? t = null;
        [NonSerialized]  
        public float? t2 = null;
        [NonSerialized]  
        public float? t3 = null;
        [NonSerialized]  
        public float? t4 = null;

        public GameObjectReference(string cellContainerId, string baseObjectId)
        {
            this.cellContainerId = cellContainerId;
            this.baseObjectId = baseObjectId;
            isOn = true;
            pos = Vector3.Zero;
            rotation = Matrix.Identity;
            rotationValues = Vector3.Zero;
            scale = new Vector3(1, 1, 1);

            physicsObj = null;
            entityTransform = Matrix.Identity;            

            target = new Vector3(0, 0, 0);
            endPoint = new Vector3(-10, 3, 10);            
            _lerpInc = 0.2f;
            color = new Vector3(1, 1, 1);
            intensity = 1;
            maxRange = 30;

            
        }


        public string BaseObjectId { get { return baseObjectId; } }
        public string CellContainerId { get { return cellContainerId; } }
        public Cell CellContainer { get { return WorldData.GetObject(cellContainerId) as Cell; } }
        public BaseGameObject BaseGameObject { get { return WorldData.GetObject(baseObjectId); } }

        public string RefId { get { return refId; } set { refId = value; } }
        public string RefName { get { return refName; } set { refName = value; } }


        public string BaseObjectType 
        {
            get 
            {
                BaseGameObject obj = BaseGameObject;
                return obj != null ? obj.GetType().Name : "";
            } 
        }

        public AnimationController AnimController
        {
            get { return animController; }
        }

        #region Attributes
        public bool getAttribute(GameObjectRefAttribute r)
        {
            initAttributes();
            return attributes.ContainsKey(r) ? attributes[r] : false;
        }
        public void setAttribute(GameObjectRefAttribute r, bool val)
        {
            initAttributes();
            attributes[r] = val;
        }
        private void initAttributes()
        {
            if (attributes == null)
            {
                attributes = new Dictionary<GameObjectRefAttribute, bool>();
                attributes[GameObjectRefAttribute.IsWater] = false;
                attributes[GameObjectRefAttribute.RecieveRainSplashes] = true;
                attributes[GameObjectRefAttribute.RecieveSnow] = true;
                attributes[GameObjectRefAttribute.UseSpecular] = true;
                attributes[GameObjectRefAttribute.UseReflectionMap] = false;
                attributes[GameObjectRefAttribute.IncludeInReflectionMap] = false;
            }
        }
        #endregion

        #region ISwitchable
        public bool IsOn { get { return isOn; } }
        public void toggle() { isOn = !isOn; }
        public void turnOn() { isOn = true; }
        public void turnOff() { isOn = false; }
        #endregion
        
        #region Physics 
        public Matrix EntityTransform
        {
            get { return entityTransform; }
            set { entityTransform = value; }
        }
        public BEPUphysics.ISpaceObject PhysicsObj
        {
            get { return physicsObj; }
            set { physicsObj = value; }
        }
        #endregion


        #region 3D data
        public Vector3 Position
        {
            get { return pos; }
            set
            {
                pos = value;
                PhysicsEngine.UpdateMeshMatrix(this);
            }
        }

        public Vector3 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                PhysicsEngine.UpdateMeshMatrix(this);
            }
        }
        public Matrix Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                PhysicsEngine.UpdateMeshMatrix(this);
            }
        }
        public Vector3 RotationValues
        {
            get { return rotationValues; }
            set { rotationValues = value; }
        }
        #endregion


        #region Lighting data
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }
        public Vector3 EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; }
        }

        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }

        public float MaxRange
        {
            get { return maxRange; }
            set { maxRange = value; }
        }

        public Vector3 LightColor
        {
            get { return color; }
        }
        public void setColor(Color3 c)
        {
            color = new Vector3(c.Red, c.Green, c.Blue);                
        }
      
        public float InnerAngle
        {
            get { return innerAngle; }
            set { innerAngle = value; }
        }
        public float OuterAngle
        {
            get { return outerAngle; }
            set { outerAngle = value; }
        }
        public float LerpInc
        {
            get { return _lerpInc; }
            set
            {
                _lerpInc = value;
                if (_lerpInc <= 0) _lerpInc = 0.1f;
            }
        }


        public float BoxHeight
        {
            get { return boxHeight; }
            set { boxHeight = value; }
        }
        public float BoxWidth
        {
            get { return boxWidth; }
            set { boxWidth = value; }
        }
        public float BoxDepth
        {
            get { return boxDepth; }
            set { boxDepth = value; }
        }        
        #endregion

        #region Materials
        public int? SelectedSubset
        {
            get { return selectedSubset; }
            set { selectedSubset = value; }
        }
        public string[] MaterialIDs
        {
            get 
            {
                if (materialIDs == null && BaseGameObject is Model)
                {
                    Model m = BaseGameObject as Model;
                    materialIDs = new string[m.Mesh3d.NumAttributes];
                }
                return materialIDs; 
            }            
        }

        public Material getMaterialForSubset(int subset)
        {
            Material mat = null;
            string[] matIds = this.MaterialIDs;
            if (matIds != null)
            {
                try { mat = WorldData.GetObject(materialIDs[subset]) as Material; }
                catch { }
            }
            if (mat == null) mat = WorldData.GetObject("DefaultMaterial") as Material;

            return mat;
        }
        public void setMaterialForSubset(int subset, Material mat)
        {
            string[] matIds = this.MaterialIDs;
            try
            {
                matIds[subset] = mat.BaseObjectId;
            }
            catch { }
            materialIDs = matIds;
        }

        
        #endregion

        #region Scripts
        public string UpdateScriptId
        {
            get { return updateScriptId; }
            set { updateScriptId = value; }
        }
        public string[] UpdateScriptArgs
        {
            get { return updateScriptArgs; }
            set { updateScriptArgs = value; }
        }
        #endregion


        public string WaterSettingId 
        { 
            get { return waterSettingId; }
            set { waterSettingId = value; }
        }
        public WaterSetting WaterSettings { get { return WorldData.GetObject(waterSettingId) as WaterSetting; } }
        


        public void Update()
        {
            if (animController == null && BaseGameObject is Model)
            {
                animController = new AnimationController(this);                
            }

            if (animController != null)
            {
                animController.update();
            }

            if (enableScripts && updateScript != null)
            {
                updateScript.Execute(this);
            }


            
            if (this.getAttribute(GameObjectRefAttribute.IsWater))
            {
                if (t == null || t2 == null || t3 == null || t4 == null)
                {
                    t = 0.0f;
                    t2 = 500.0f;
                    t3 = 750.0f;
                    t4 = 1000.0f;
                }

                WaterSetting ws = WaterSettings;
                if (ws == null) ws = WorldData.GetObject("DefaultWater") as WaterSetting;  
                if (t < 1000.0f)
                    t += ws.Scroll;
                else
                    t = 0;

                if (t2 < 2000.0f)
                    t2 += ws.Scroll2;
                else
                    t2 = 0;

                if (t3 < 3000.0f)
                    t3 += ws.Scroll3;
                else
                    t3 = 0;

                if (t4 < 2500.0f)
                    t4 += ws.Scroll4;
                else
                    t4 = 0;
            }
        }

        public void Dispose()
        {
            
        }

    }
}
