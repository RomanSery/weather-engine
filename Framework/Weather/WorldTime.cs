using System;

namespace WeatherGame.Framework.Weather
{
    public static class WorldTime
    {
        public static DateTime time;
        public static int Sunrise = 7;
        public static int Sunset = 18;
        public static int Speed = 2;

        static WorldTime()
        {
            time = new DateTime(2006, 1, 1, 11, 0, 0);
        }

        public static int getNumSecondsTill(int targetHour)
        {
            int h = 0;
            if (Hour < targetHour)
                h = targetHour - Hour;
            else
                h = targetHour + (24 - Hour);

            return (h * 60 * 60) - (Minute * 60) - Second;
        }


        public static long getNumSecondsElapsedInDay()
        {
            return (Hour * 60 * 60) + (Minute * 60) + Second;
        }

        public static long getNumSecondsElapsedFromSunriseToSunset()
        {

            if (isTimeBetween(Sunrise, Sunset) == false) return 0;

            return getNumSecondsElapsedInDay() - (Sunrise * 60 * 60);
        }

        public static long getNumSecondsElapsedFromHourToHour(int h1, int h2)
        {
            if (isTimeBetween(h1, h2) == false) return 0;

            if (h2 > h1)
            {
                return getNumSecondsElapsedInDay() - (h1 * 60 * 60);
            }
            else
            {
                return 0;
            }
        }

        public static float getNumSecondsFromSunriseToSunset()
        {
            return getNumSecondsFromHourToHour(Sunrise, Sunset);
        }

        public static float getNumSecondsFromHourToHour(int h1, int h2)
        {
            if (h2 > h1)
            {
                return (h2 - h1) * 60 * 60;
            }
            else
            {
                long n1 = (24 - h1) * 60 * 60;
                long n2 = h2 * 60 * 60;
                return n1 + n2;
            }
        }

        public static bool isTimeBetween(int h1, int h2)
        {
            int h = Hour;
            bool result = false;
            if (h2 > h1)
            {
                if (h >= h1 && h < h2) result = true;
            }
            else if (h1 > h2)
            {
                if (h >= h1 || h < h2) result = true;
            }
            else if (h1 == h2)
            {
                if (h == h1) result = true;
            }
            return result;
        }


        public static void Update()
        {
            time = time.AddSeconds(Speed);

        }

        public static int getNumSecondsTillNoon() { return getNumSecondsTill(12); }
        public static int getNumSecondsTillSunset() { return getNumSecondsTill(19); }
        public static int getNumSecondsTillMidnight() { return getNumSecondsTill(24); }



        #region Properties
        public static int Hour { get { return time.Hour; } }
        public static int Minute { get { return time.Minute; } }
        public static int Second { get { return time.Second; } }
        #endregion
    }
}
