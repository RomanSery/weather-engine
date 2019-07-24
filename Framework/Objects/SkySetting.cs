using System;
using SlimDX;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class SkySetting : BaseGameObject
    {
        private float cloudCover;
        private Vector2 cloudTile;
        private Vector2 scroll;
        private int numSamples;
        private float exposure;
        private Vector3 waveLengths;
        private float fogDensity;  

        public SkySetting()
        {
            canHaveRefs = false;
            NumSamples = 20;            
            FogDensity = 0.0007f;
            waveLengths = new Vector3(0.65f, 0.57f, 0.475f);
            Exposure = -2.0f;            

            cloudTile.X = 2;
            cloudTile.Y = 1;
            scroll = new Vector2(0.0001f, 0.0002f);
            cloudCover = 1.0f;           
        }

        public float CloudCover
        {
            get { return cloudCover; }
            set { cloudCover = value; }
        }

        public Vector2 CloudTile
        {
            get { return cloudTile; }
            set { cloudTile = value; }
        }

        public Vector2 Scroll
        {
            get { return scroll; }
            set { scroll = value; }
        }

        public int NumSamples
        {
            get { return numSamples; }
            set { numSamples = value; }
        }

        public float Exposure
        {
            get { return exposure; }
            set { exposure = value; }
        }

        public Vector3 WaveLengths
        {
            get { return waveLengths; }
            set { waveLengths = value; }
        }

        public float FogDensity
        {
            get { return fogDensity; }
            set { fogDensity = value; }
        }

        public override void Dispose()
        {

        }



    }
}
