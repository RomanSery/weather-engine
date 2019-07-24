using System;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class BloomSetting : BaseGameObject
    {
        private float blurAmount = 2; //1 -10
        private float bloomThreshold = 0.25f; //Somewhere between 0.25 and 0.5 is good.
        private float bloomIntensity = 1; //Range 0 to 1.
        private float baseIntensity = 1;
        private float bloomSaturation = 1;
        private float baseSaturation = 1;

        public BloomSetting()
        {
            canHaveRefs = false;
        }

        public float BlurAmount
        {
            get { return blurAmount; }
            set { blurAmount = value; }
        }

        public float BloomThreshold
        {
            get { return bloomThreshold; }
            set { bloomThreshold = value; }
        }

        public float BloomIntensity
        {
            get { return bloomIntensity; }
            set { bloomIntensity = value; }
        }

        public float BaseIntensity
        {
            get { return baseIntensity; }
            set { baseIntensity = value; }
        }

        public float BloomSaturation
        {
            get { return bloomSaturation; }
            set { bloomSaturation = value; }
        }

        public float BaseSaturation
        {
            get { return baseSaturation; }
            set { baseSaturation = value; }
        }

        public override void Dispose()
        {

        }

    }
}
