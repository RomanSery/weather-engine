using System;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class WaterSetting : BaseGameObject
    {

        private float scroll = 0.0001f;
        private float scroll2 = 0.0002f;
        private float scroll3 = 0.0002f;
        private float scroll4 = 0.0001f;
        private float shoreFalloff = 2.0f;
        private float shoreScale = 5.0f;
        private float speed = 5;
        private float reflectionFactorOffset = 0.5f;

        private float t = 0.0f;
        private float t2 = 500.0f;
        private float t3 = 750.0f;
        private float t4 = 1000.0f;

        public WaterSetting()
        {
            canHaveRefs = false;

            scroll = 0.0001f;
            scroll2 = 0.0002f;
            scroll3 = 0.0002f;
            scroll4 = 0.0001f;
            shoreFalloff = 2.0f;
            shoreScale = 5.0f;
            speed = 5;
            reflectionFactorOffset = 0.5f;

            t = 0.0f;
            t2 = 500.0f;
            t3 = 750.0f;
            t4 = 1000.0f;
        }

        public float Scroll
        {
            get { return scroll; }
            set { scroll = value; }
        }

        public float Scroll2
        {
            get { return scroll2; }
            set { scroll2 = value; }
        }

        public float Scroll3
        {
            get { return scroll3; }
            set { scroll3 = value; }
        }

        public float Scroll4
        {
            get { return scroll4; }
            set { scroll4 = value; }
        }

        public float ShoreFalloff
        {
            get { return shoreFalloff; }
            set { shoreFalloff = value; }
        }

        public float ShoreScale
        {
            get { return shoreScale; }
            set { shoreScale = value; }
        }

        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public float ReflectionFactorOffset
        {
            get { return reflectionFactorOffset; }
            set { reflectionFactorOffset = value; }
        }


        private void Update()
        {
            if (t < 1000.0f)
                t += scroll;
            else
                t = 0;

            if (t2 < 2000.0f)
                t2 += scroll2;
            else
                t2 = 0;

            if (t3 < 3000.0f)
                t3 += scroll3;
            else
                t3 = 0;

            if (t4 < 2500.0f)
                t4 += scroll4;
            else
                t4 = 0;
        }

        public override void Dispose()
        {

        }
    }
}
