using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeatherGame.Framework;
using WorldEditor.Global;
using WeatherGame.Framework.physics;
using WeatherGame.Framework.Player;
using SlimDX;
using WeatherGame.Framework.Weather;
using WeatherGame.Framework.World;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Scripting;

namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {        
        public delegate void SaveLoadCellHandler(string filename);
        public event SaveLoadCellHandler SaveWorld;
        public event SaveLoadCellHandler LoadWorld;

        public MainMenu()
        {
            InitializeComponent();

            SlimDX.Vector4 c = GlobalSettings.GridColor;
            colorFill.Color = Color.FromArgb(Convert.ToByte(c.W * 255), Convert.ToByte(c.X * 255), Convert.ToByte(c.Y * 255), Convert.ToByte(c.Z * 255));
            gridPos.NumericValue = Rendering.Grid.Position.Y;

            chkRenderLights.IsChecked = GlobalSettings.RenderLights;
            chkRenderGrid.IsChecked = GlobalSettings.RenderGrid;
            chkRenderVolume.IsChecked = GlobalSettings.RenderVolumes;
            chkPhysics.IsChecked = !PhysicsEngine.Paused;
            chkPhysicsCamera.IsChecked = false;

            List<BaseGameObject> weatherSettings = WorldData.GetObjectsByType(typeof(WeatherSetting));            
            foreach (BaseGameObject s in weatherSettings)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = s.BaseObjectId;
                i.Content = s.BaseObjectName;
                i.IsSelected = s.BaseObjectId == "defaultWeather";
                ddlSetWeather.Items.Add(i);                
            }
        }            

        private void chkRenderLights_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.RenderLights = (chkRenderLights.IsChecked.HasValue && chkRenderLights.IsChecked.Value);
        }
        private void chkRenderGrid_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.RenderGrid = (chkRenderGrid.IsChecked.HasValue && chkRenderGrid.IsChecked.Value);
        }
        private void chkRenderVolume_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.RenderVolumes = (chkRenderVolume.IsChecked.HasValue && chkRenderVolume.IsChecked.Value);
        }
        private void chkPhysics_Click(object sender, RoutedEventArgs e)
        {
            if (chkPhysics.IsChecked.HasValue && chkPhysics.IsChecked.Value)
                PhysicsEngine.Paused = false;
            else
                PhysicsEngine.Paused = true;
        }

        protected void gridPos_NumericChanged(object sender, EventArgs e)
        {
            Rendering.Grid.Position.Y = gridPos.NumericValue;
        }

        private void chkPhysicsCamera_Click(object sender, RoutedEventArgs e)
        {
            if (chkPhysicsCamera.IsChecked.HasValue && chkPhysicsCamera.IsChecked.Value)
            {
                MainPlayer.Controller.ActivatePhysics();
                chkPhysics.IsChecked = true;
            }
            else
            {
                MainPlayer.Controller.DeactivatePhysics();
                chkPhysics.IsChecked = false;
            }
            chkPhysics_Click(sender, e);
        }


        private void metersPerSquare_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int n = Convert.ToInt32(((ComboBoxItem)e.AddedItems[0]).Content);
                Rendering.Grid.MetersPerSquare = n;
                Rendering.Grid.Initialize();
            }
            catch { }
        }

        protected void worldTimePicker_TimeChanged(object sender, EventArgs e)
        {
            WorldTime.time = worldTimePicker.SelectedTime;
        }


        private void ChangeLightColor(object sender, RoutedEventArgs e)
        {
            ColorPickerControl.ColorPickerDialog cPicker = new ColorPickerControl.ColorPickerDialog();
            cPicker.ColorChanging += new RoutedPropertyChangedEventHandler<Color>(cPicker_ColorChanging);


            SlimDX.Vector4 c = GlobalSettings.GridColor;
            cPicker.StartingColor = Color.FromArgb(Convert.ToByte(c.W * 255), Convert.ToByte(c.X * 255), Convert.ToByte(c.Y * 255), Convert.ToByte(c.Z * 255)); ;
            cPicker.Owner = Window.GetWindow(this);

            bool? dialogResult = cPicker.ShowDialog();
            if (dialogResult != null && (bool)dialogResult == true)
            {
                Color sc = cPicker.SelectedColor;
                colorFill.Color = sc;
                Color4 c3 = new Color4((float)(sc.A / 255.0), (float)(sc.R / 255.0), (float)(sc.G / 255.0), (float)(sc.B / 255.0));
                GlobalSettings.GridColor = new Vector4(c3.Red, c3.Green, c3.Blue, c3.Alpha);

            }
        }
        void cPicker_ColorChanging(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Color sc = e.NewValue;
            Color4 c3 = new Color4((float)(sc.A / 255.0), (float)(sc.R / 255.0), (float)(sc.G / 255.0), (float)(sc.B / 255.0));
            GlobalSettings.GridColor = new Vector4(c3.Red, c3.Green, c3.Blue, c3.Alpha);
        }

        private void cameraSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Camera.Velocity = new Vector3((float)cameraSpeed.Value);
        }

     

        private void save_Click(object sender, RoutedEventArgs e)
        {
            //if (SaveWorld != null)
                //SaveWorld("");  
            ScriptManager.SetExecScripts(false);  

            GameData.SaveGameData();          
        }

        private void ddlSetWeather_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbItemWeather = ddlSetWeather.SelectedItem as ComboBoxItem;
            SkyDome.SetSettings(WorldData.GetObject(cbItemWeather.Name.ToString()) as WeatherSetting);            
        }

        private void chkExecScripts_Click(object sender, RoutedEventArgs e)
        {            
            GlobalSettings.ExecuteScripts = (chkExecScripts.IsChecked.HasValue && chkExecScripts.IsChecked.Value);
            ScriptManager.SetExecScripts(GlobalSettings.ExecuteScripts);
        }
            
    }
}
