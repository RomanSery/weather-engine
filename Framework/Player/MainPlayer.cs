using WeatherGame.Framework.Objects;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Player
{
    public static class MainPlayer
    {
        private static WorldSpace currWorld = null;
        private static Cell currCell = null;
        private static PlayerInput mainPlayerController = null;


        static MainPlayer()
        {
            mainPlayerController = new PlayerInput();
        }

        public static WorldSpace CurrentWorldSpace
        {
            get
            {
                if (currWorld == null)
                {
                    GlobalVar defaultWorldVar = WorldData.GetObject("var_DefaultWorld") as GlobalVar;
                    if(defaultWorldVar != null)
                        currWorld = WorldData.GetObject(defaultWorldVar.StringValue) as WorldSpace;
                }
                return currWorld;
            }
        }

        public static Cell CurrentCell
        {
            get
            {
                if (currCell == null)
                {
                    currCell = CurrentWorldSpace.AllCells[0];                    
                }
                return currCell;
            }
        }

        public static PlayerInput Controller
        {
            get
            {
                return mainPlayerController;
            }
        }


        public static void Initialize()
        {
            mainPlayerController.Initialize();
        }

        public static void Update()
        {
            mainPlayerController.Update();
        }
    }
}
