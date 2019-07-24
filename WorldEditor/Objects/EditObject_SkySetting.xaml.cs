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
using SlimDX;

namespace WorldEditor.Popups
{
    /// <summary>
    /// Interaction logic for EditObject_SkySetting.xaml
    /// </summary>
    public partial class EditObject_SkySetting : CustomWindow
    {
        private bool isEditingObj = false;
        private SkySetting cancelRevert = null;

        public EditObject_SkySetting()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is SkySetting)
            {
                SkySetting obj = objectToEdit as SkySetting;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                initValues(obj);
                cancelRevert = (SkySetting)Utils.DeepClone(obj);  
            }
            else
            {
                SkySetting ss = new SkySetting();                
                initValues(ss);
            }
        }

        private void initValues(SkySetting mObj)
        {
            cloudCover.Value = mObj.CloudCover;
            cloudTitle1.Value = mObj.CloudTile.X;
            cloudTitle2.Value = mObj.CloudTile.Y;
            cloudScroll1.Value = mObj.Scroll.X;
            cloudScroll2.Value = mObj.Scroll.Y;
            numSamples.Value = mObj.NumSamples;
            exposure.Value = mObj.Exposure;
            waveLenX.Value = mObj.WaveLengths.X;
            waveLenY.Value = mObj.WaveLengths.Y;
            waveLenZ.Value = mObj.WaveLengths.Z;
            fogDensity.Value = mObj.FogDensity;
        }

        private void cloudCover_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SkySetting)objectToEdit).CloudCover = (float)e.NewValue;
        }
        private void cloudTitle1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector2 v = ((SkySetting)objectToEdit).CloudTile;
                v.X = (float)e.NewValue;
                ((SkySetting)objectToEdit).CloudTile = v;
            }
        }
        private void cloudTitle2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector2 v = ((SkySetting)objectToEdit).CloudTile;
                v.Y = (float)e.NewValue;
                ((SkySetting)objectToEdit).CloudTile = v;
            }
        }
        private void cloudScroll1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector2 v = ((SkySetting)objectToEdit).Scroll;
                v.X = (float)e.NewValue;
                ((SkySetting)objectToEdit).Scroll = v;
            }
        }
        private void cloudScroll2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector2 v = ((SkySetting)objectToEdit).Scroll;
                v.Y = (float)e.NewValue;
                ((SkySetting)objectToEdit).Scroll = v;
            }
        }
        private void numSamples_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SkySetting)objectToEdit).NumSamples = (int)e.NewValue;
        }
        private void exposure_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SkySetting)objectToEdit).Exposure = (float)e.NewValue;
        }
        private void waveLenX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector3 v = ((SkySetting)objectToEdit).WaveLengths;
                v.X = (float)e.NewValue;
                ((SkySetting)objectToEdit).WaveLengths = v;
            }
        }
        private void waveLenY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector3 v = ((SkySetting)objectToEdit).WaveLengths;
                v.Y = (float)e.NewValue;
                ((SkySetting)objectToEdit).WaveLengths = v;
            }
        }
        private void waveLenZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null)
            {
                Vector3 v = ((SkySetting)objectToEdit).WaveLengths;
                v.Z = (float)e.NewValue;
                ((SkySetting)objectToEdit).WaveLengths = v;
            }
        }
        private void fogDensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((SkySetting)objectToEdit).FogDensity = (float)e.NewValue;
        }




        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new SkySetting();

            SkySetting obj = objectToEdit as SkySetting;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            //custom code
            obj.CloudCover = (float)cloudCover.Value;
            obj.CloudTile = new Vector2((float)cloudTitle1.Value, (float)cloudTitle2.Value);
            obj.Scroll = new Vector2((float)cloudScroll1.Value, (float)cloudScroll2.Value);                        
            obj.NumSamples = (int)numSamples.Value;
            obj.Exposure = (float)exposure.Value;

            Vector3 wl = new Vector3((float)waveLenX.Value, (float)waveLenY.Value, (float)waveLenZ.Value);
            obj.WaveLengths = wl;
            obj.FogDensity = (float)fogDensity.Value;        
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
