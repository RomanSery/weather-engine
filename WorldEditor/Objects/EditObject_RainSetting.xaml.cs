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
    /// Interaction logic for EditObject_RainSetting.xaml
    /// </summary>
    public partial class EditObject_RainSetting : CustomWindow
    {
        private bool isEditingObj = false;
        private RainSetting cancelRevert = null;

        public EditObject_RainSetting()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is RainSetting)
            {
                RainSetting obj = objectToEdit as RainSetting;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                initValues(obj);
                cancelRevert = (RainSetting)Utils.DeepClone(obj);  
            }
            else
            {
                RainSetting rs = new RainSetting();
                initValues(rs);
            }
        }

        private void initValues(RainSetting mObj)
        {
            rainSpeed.Value = mObj.RainSpeed;
            dirLightIntensity.Value = mObj.DirLightIntensity;
            responseDirLight.Value = mObj.ResponseDirLight;
            rainSplashSpeed.Value = mObj.RainSplashSpeed;
            rainSplashSize.Value = mObj.RainSplashSize;
            percentDrawParticles.Value = mObj.PercentDrawParticles;
        }



        private void percentDrawParticles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((RainSetting)objectToEdit).PercentDrawParticles = (float)e.NewValue;
        }
        private void rainSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((RainSetting)objectToEdit).RainSpeed = (float)e.NewValue;
        }
        private void dirLightIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((RainSetting)objectToEdit).DirLightIntensity = (float)e.NewValue;
        }
        private void responseDirLight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((RainSetting)objectToEdit).ResponseDirLight = (float)e.NewValue;
        }
        private void rainSplashSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((RainSetting)objectToEdit).RainSplashSpeed = (float)e.NewValue;
        }
        private void rainSplashSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((RainSetting)objectToEdit).RainSplashSize = (float)e.NewValue;
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new RainSetting();

            RainSetting obj = objectToEdit as RainSetting;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            //custom code
            obj.RainSpeed = (float)rainSpeed.Value;
            obj.DirLightIntensity = (float)dirLightIntensity.Value;
            obj.ResponseDirLight = (float)responseDirLight.Value;
            obj.RainSplashSpeed = (float)rainSplashSpeed.Value;
            obj.RainSplashSize = (float)rainSplashSize.Value;
            obj.PercentDrawParticles = (float)percentDrawParticles.Value;            
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
