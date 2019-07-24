using System;

namespace WeatherGame.Framework.Objects
{


    [Serializable]
    public class SnowSetting : BaseGameObject
    {
        private float snowSpeed;
        private float percentDrawParticles;
        private float snowValue;
        private float wallSnowValue;
        private float turbulence;
        private float g_SpriteSize;

        public SnowSetting()
        {
            canHaveRefs = false;
            snowSpeed = 0.4f;
            percentDrawParticles = 1;
            turbulence = 0.1f;
            g_SpriteSize = 0.09f;
            snowValue = 1.0f;
            wallSnowValue = 1.0f;
        }

        public float SnowSpeed
        {
            get { return snowSpeed; }
            set { snowSpeed = value; }
        }

        public float PercentDrawParticles
        {
            get { return percentDrawParticles; }
            set { percentDrawParticles = value; }
        }

        public float SnowValue
        {
            get { return snowValue; }
            set { snowValue = value; }
        }

        public float WallSnowValue
        {
            get { return wallSnowValue; }
            set { wallSnowValue = value; }
        }

        public float Turbulence
        {
            get { return turbulence; }
            set { turbulence = value; }
        }

        public float SpriteSize
        {
            get { return g_SpriteSize; }
            set { g_SpriteSize = value; }
        }

        public override void Dispose()
        {

        }
    }
}
