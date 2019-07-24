using System;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class RainSetting : BaseGameObject
    {
        private float rainSpeed;
        private float dirLightIntensity;
        private float responseDirLight;
        private float rainSplashSpeed;
        private float rainSplashSize;
        private float percentDrawParticles;

        public RainSetting()
        {
            canHaveRefs = false;
            rainSpeed = 2;
            dirLightIntensity = 0.18f;
            responseDirLight = 1.0f;
            rainSplashSpeed = 0.075f;
            rainSplashSize = 0.2f;
            percentDrawParticles = 1;
        }

        public float RainSpeed
        {
            get { return rainSpeed; }
            set { rainSpeed = value; }
        }

        public float DirLightIntensity
        {
            get { return dirLightIntensity; }
            set { dirLightIntensity = value; }
        }

        public float ResponseDirLight
        {
            get { return responseDirLight; }
            set { responseDirLight = value; }
        }

        public float RainSplashSpeed
        {
            get { return rainSplashSpeed; }
            set { rainSplashSpeed = value; }
        }

        public float RainSplashSize
        {
            get { return rainSplashSize; }
            set { rainSplashSize = value; }
        }

        public float PercentDrawParticles
        {
            get { return percentDrawParticles; }
            set { percentDrawParticles = value; }
        }

        public override void Dispose()
        {
         
        }
    }
}
