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
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using WeatherGame;
using WeatherGame.Framework;
using WeatherGame.Framework.Rendering;
using WeatherGame.RenderLoop;
using WeatherGame.BuildContent;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Objects;
using WorldEditor.Global;
using WeatherGame.Framework.World;

namespace WorldEditor.Popups
{
    /// <summary>
    /// Interaction logic for EditObject_WeatherSetting.xaml
    /// </summary>
    public partial class EditObject_WeatherSetting : CustomWindow
    {
        private bool isEditingObj = false;
        private WeatherSetting cancelRevert = null;

        public EditObject_WeatherSetting()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is WeatherSetting)
            {
                WeatherSetting obj = objectToEdit as WeatherSetting;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;

                chkRainIsOn.IsChecked = obj.IsRainOn;
                chkSnowIsOn.IsChecked = obj.IsSnowOn;
                chkLightningIsOn.IsChecked = obj.IsLightningOn;

                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                cancelRevert = (WeatherSetting)Utils.DeepClone(obj); 
            }


            List<BaseGameObject> rainSettings = WorldData.GetObjectsByType(typeof(RainSetting));
            int x = 0;
            foreach (BaseGameObject s in rainSettings)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = s.BaseObjectId;
                i.Content = s.BaseObjectName;
                i.IsSelected = false;
                if (objectToEdit != null && objectToEdit is WeatherSetting)
                {
                    if (((WeatherSetting)objectToEdit).RainSettingId == s.BaseObjectId) i.IsSelected = true;
                }
                ddlRain.Items.Add(i);
                x++;
            }

            List<BaseGameObject> snowSettings = WorldData.GetObjectsByType(typeof(SnowSetting));
            x = 0;
            foreach (BaseGameObject s in snowSettings)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = s.BaseObjectId;
                i.Content = s.BaseObjectName;
                i.IsSelected = false;
                if (objectToEdit != null && objectToEdit is WeatherSetting)
                {
                    if (((WeatherSetting)objectToEdit).SnowSettingId == s.BaseObjectId) i.IsSelected = true;
                }
                ddlSnow.Items.Add(i);
                x++;
            }

            List<BaseGameObject> skySettings = WorldData.GetObjectsByType(typeof(SkySetting));
            x = 0;
            foreach (BaseGameObject s in skySettings)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = s.BaseObjectId;
                i.Content = s.BaseObjectName;
                i.IsSelected = false;
                if (objectToEdit != null && objectToEdit is WeatherSetting)
                {
                    if (((WeatherSetting)objectToEdit).SkySettingId == s.BaseObjectId) i.IsSelected = true;
                }
                ddlSky.Items.Add(i);
                x++;
            }  
        }


        private void chkRainIsOn_Checked(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null) ((WeatherSetting)objectToEdit).IsRainOn = (chkRainIsOn.IsChecked.HasValue && chkRainIsOn.IsChecked.Value) ? true : false;
        }
        private void chkLightningIsOn_Checked(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null) ((WeatherSetting)objectToEdit).IsLightningOn = (chkLightningIsOn.IsChecked.HasValue && chkLightningIsOn.IsChecked.Value) ? true : false;
        }

        
        private void ddlRain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ComboBoxItem cbItemRain = ddlRain.SelectedItem as ComboBoxItem;
                ((WeatherSetting)objectToEdit).RainSettingId = cbItemRain.Name.ToString();
            }
        }
        private void chkSnowIsOn_Checked(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null) ((WeatherSetting)objectToEdit).IsSnowOn = (chkSnowIsOn.IsChecked.HasValue && chkSnowIsOn.IsChecked.Value) ? true : false;
        }
        private void ddlSnow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ComboBoxItem cbItemSnow = ddlSnow.SelectedItem as ComboBoxItem;
                ((WeatherSetting)objectToEdit).SnowSettingId = cbItemSnow.Name.ToString();
            }
        }
        private void ddlSky_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ComboBoxItem cbItemSky = ddlSky.SelectedItem as ComboBoxItem;
                ((WeatherSetting)objectToEdit).SkySettingId = cbItemSky.Name.ToString();
            }
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new WeatherSetting();

            WeatherSetting obj = objectToEdit as WeatherSetting;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            ComboBoxItem cbItemRain = ddlRain.SelectedItem as ComboBoxItem;
            ComboBoxItem cbItemSnow = ddlSnow.SelectedItem as ComboBoxItem;
            ComboBoxItem cbItemSky = ddlSky.SelectedItem as ComboBoxItem;           
            obj.RainSettingId = cbItemRain.Name.ToString();
            obj.SnowSettingId = cbItemSnow.Name.ToString();
            obj.SkySettingId = cbItemSky.Name.ToString();

            obj.IsRainOn = (chkRainIsOn.IsChecked.HasValue && chkRainIsOn.IsChecked.Value) ? true : false;
            obj.IsSnowOn = (chkSnowIsOn.IsChecked.HasValue && chkSnowIsOn.IsChecked.Value) ? true : false;
            obj.IsLightningOn = (chkLightningIsOn.IsChecked.HasValue && chkLightningIsOn.IsChecked.Value) ? true : false;

            obj.Category = objCategory;
            bool success = isEditingObj ? WorldData.UpdateObject(obj) : WorldData.AddObject(obj);
            if (!success)
                MessageBox.Show(Application.Current.MainWindow, "ID already exists", "AddObject Failure", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Cancel);
            else
            {
                this.Close();
                if (parentWindow != null) parentWindow.Refresh();
            }


        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (cancelRevert != null && objectToEdit != null)
            {
                WorldData.UpdateObject(cancelRevert);
            }
            this.Close();
            if (parentWindow != null) parentWindow.Refresh();
        }

        



        

    }
}
