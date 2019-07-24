using System;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Objects
{
    [Serializable]
    public class WeatherSetting : BaseGameObject
    {
        private string rainSettingId = null;
        private string snowSettingId = null;
        private string skySettingId = null;

        private bool isRainOn = false;
        private bool isSnowOn = false;
        private bool isLightningOn = false;

        public WeatherSetting()
        {
            canHaveRefs = false;
           
        }

        public RainSetting RainSettings { get { return WorldData.GetObject(rainSettingId) as RainSetting; } }
        public SnowSetting SnowSettings { get { return WorldData.GetObject(snowSettingId) as SnowSetting; } }
        public SkySetting SkySettings { get { return WorldData.GetObject(skySettingId) as SkySetting; } }

        public string RainSettingId
        {
            get { return rainSettingId; }
            set { rainSettingId = value; }
        }

        public string SnowSettingId
        {
            get { return snowSettingId; }
            set { snowSettingId = value; }
        }

        public string SkySettingId
        {
            get { return skySettingId; }
            set { skySettingId = value; }
        }

        public bool IsRainOn
        {
            get { return isRainOn; }
            set { isRainOn = value; }
        }

        public bool IsSnowOn
        {
            get { return isSnowOn; }
            set { isSnowOn = value; }
        }

        public bool IsLightningOn
        {
            get { return isLightningOn; }
            set { isLightningOn = value; }
        }

        public override void Dispose()
        {

        }
    }
}
