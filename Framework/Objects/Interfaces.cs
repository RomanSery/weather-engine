using SlimDX;

namespace WeatherGame.Framework.Interfaces
{   
    public interface IPositionable
    {
        Vector3 Position { get; set; }
    }

    public interface ISwitchable
    {
        bool IsOn { get; }

        void toggle();
        void turnOn();
        void turnOff();
    }
}
