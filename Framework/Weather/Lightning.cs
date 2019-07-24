using System.Collections.Generic;
using SlimDX;
using WeatherGame.RenderLoop;
using System;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;

namespace WeatherGame.Framework.Weather
{
    public struct sLightningStrikePattern
    {
        public float[] n;
        public int numSteps;

        public sLightningStrikePattern(int i)
        {
            numSteps = 0;
            n = new float[20];
        }
    };

    public static class Lightning
    {
        public static bool IsOn = false;
        public static bool RenderLight = false;

        public static Vector3 LightningPos;
        public static Vector3 LightningColor;
        public static float LightningRadius;

        private static float targetTime = 0.0f;
        private static float lightningTime = 0.0f;
        private static float time = 0.0f;

        private static int index = 0;
        private static bool gotTargetTime = false;
        private static bool playedStrikeSound = false;
        private static Random r = new Random();

        private static sLightningStrikePattern[] lightningPatterns = new sLightningStrikePattern[2];
        private static sLightningStrikePattern targetPattern;


        public static void Initalize()
        {
            lightningPatterns[0].n = new float[6];
            lightningPatterns[0].n[0] = 0.0f;
            lightningPatterns[0].n[1] = 1.5f;
            lightningPatterns[0].n[2] = 2.0f;
            lightningPatterns[0].n[3] = 2.2f;
            lightningPatterns[0].n[4] = 3.5f;
            lightningPatterns[0].n[5] = 3.7f;
            lightningPatterns[0].numSteps = 6;

            lightningPatterns[1].n = new float[2];
            lightningPatterns[1].n[0] = 0.0f;
            lightningPatterns[1].n[1] = 1.5f;
            lightningPatterns[1].numSteps = 2;

            LightningPos = new Vector3(0, 200, 0);
            LightningColor = new Vector3(0, 0, 1);
            LightningRadius = 2500;
        }

        //returns true if finished with strike, false otherwise
        private static bool doLightningStrike()
        {
            if (lightningTime >= targetPattern.n[index])
            {
                RenderLight = !RenderLight;
                index++;

                if (index == targetPattern.numSteps)
                {
                    index = 0;
                    lightningTime = 0.0f;
                    RenderLight = false;
                    return true;
                }
            }

            lightningTime += Game.Time.ElapsedGameTime;
            return false;
        }

        public static void Update()
        {
            if (!IsOn) return;

            if (gotTargetTime == false)
            {
                targetTime = r.Next(5, 15);
                gotTargetTime = true;
                time = 0;
                targetPattern = lightningPatterns[0];
            }
            else
            {
                if (time >= targetTime)
                {
                    if (playedStrikeSound == false)
                    {
                        //play sound here
                        playedStrikeSound = true;
                    }
                    //check if finished with strike
                    if (doLightningStrike())
                    {
                        gotTargetTime = false;
                        playedStrikeSound = false;
                    }
                }
                time = time + Game.Time.ElapsedGameTime;
            }
        }

        #region Helpers
        public static Matrix GetLightVolumeWVP()
        {
            return GetLightVolumeWorldMatrix() * Camera.ViewMatrix * Camera.ProjectionMatrix;
        }
        private static Matrix GetLightVolumeWorldMatrix()
        {
            float fScale = LightningRadius * 1.1f;
            Matrix scale, translate, world = Matrix.Identity;
            translate = WorldSpace.GetRealWorldMatrix(LightningPos, MainPlayer.CurrentCell);
            scale = Matrix.Scaling(fScale, fScale, fScale);
            world = Matrix.Multiply(world, scale);
            world = Matrix.Multiply(world, translate);
            return world;
        }
        #endregion

    }
}
