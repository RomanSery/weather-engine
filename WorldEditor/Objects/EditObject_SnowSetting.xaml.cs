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
    /// Interaction logic for EditObject_SnowSetting.xaml
    /// </summary>
    public partial class EditObject_SnowSetting : CustomWindow
    {
        private bool isEditingObj = false;
        private SnowSetting cancelRevert = null;

        public EditObject_SnowSetting()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is SnowSetting)
            {
                SnowSetting obj = objectToEdit as SnowSetting;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                initValues(obj);
                cancelRevert = (SnowSetting)Utils.DeepClone(obj);  
            }
            else
            {
                SnowSetting ss = new SnowSetting();
                initValues(ss);
            }
        }

        private void initValues(SnowSetting mObj)
        {
            snowValue.Value = mObj.SnowValue;
            snowWallValue.Value = mObj.WallSnowValue;
            snowSpeed.Value = mObj.SnowSpeed;
            percentDrawParticlesSnow.Value = mObj.PercentDrawParticles;
            turbulenceValue.Value = mObj.Turbulence;
            spriteSizeSnow.Value = mObj.SpriteSize;
        }



        private void percentDrawParticlesSnow_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SnowSetting)objectToEdit).PercentDrawParticles = (float)e.NewValue;
        }
        private void snowSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SnowSetting)objectToEdit).SnowSpeed = (float)e.NewValue;
        }
        private void snowValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SnowSetting)objectToEdit).SnowValue = (float)e.NewValue;
        }
        private void snowWallValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SnowSetting)objectToEdit).WallSnowValue = (float)e.NewValue;
        }
        private void turbulenceValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SnowSetting)objectToEdit).Turbulence = (float)e.NewValue;
        }
        private void spriteSizeSnow_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SnowSetting)objectToEdit).SpriteSize = (float)e.NewValue;
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new SnowSetting();

            SnowSetting obj = objectToEdit as SnowSetting;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            //custom code
            obj.SnowValue = (float)snowValue.Value;
            obj.WallSnowValue = (float)snowWallValue.Value;
            obj.SnowSpeed = (float)snowSpeed.Value;
            obj.PercentDrawParticles = (float)percentDrawParticlesSnow.Value;
            obj.Turbulence = (float)turbulenceValue.Value;
            obj.SpriteSize = (float)spriteSizeSnow.Value;            
            //custom code end

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
