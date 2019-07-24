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
    /// Interaction logic for EditObject_WaterSetting.xaml
    /// </summary>
    public partial class EditObject_WaterSetting : CustomWindow
    {
        private bool isEditingObj = false;
        private WaterSetting cancelRevert = null;

        public EditObject_WaterSetting()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is WaterSetting)
            {
                WaterSetting obj = objectToEdit as WaterSetting;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                initValues(obj);
                cancelRevert = (WaterSetting)Utils.DeepClone(obj);
            }
            else
            {
                WaterSetting ss = new WaterSetting();
                initValues(ss);
            }
        }

        private void initValues(WaterSetting mObj)
        {
            scroll.Value = mObj.Scroll;
            scroll2.Value = mObj.Scroll2;
            scroll3.Value = mObj.Scroll3;
            scroll4.Value = mObj.Scroll4;
            shoreFalloff.Value = mObj.ShoreFalloff;
            shoreScale.Value = mObj.ShoreScale;
            speed.Value = mObj.Speed;
            reflectionFactorOffset.Value = mObj.ReflectionFactorOffset;
        }

        private void scroll_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).Scroll = (float)e.NewValue;
        }
        private void scroll2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).Scroll2 = (float)e.NewValue;
        }
        private void scroll3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).Scroll3 = (float)e.NewValue;
        }
        private void scroll4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).Scroll4 = (float)e.NewValue;
        }
        private void shoreFalloff_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).ShoreFalloff = (float)e.NewValue;
        }
        private void shoreScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).ShoreScale = (float)e.NewValue;
        }
        private void speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).Speed = (float)e.NewValue;
        }
        private void reflectionFactorOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((WaterSetting)objectToEdit).ReflectionFactorOffset = (float)e.NewValue;
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new WaterSetting();

            WaterSetting obj = objectToEdit as WaterSetting;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            //custom code
            obj.Scroll = (float)scroll.Value;
            obj.Scroll2 = (float)scroll2.Value;
            obj.Scroll3 = (float)scroll3.Value;
            obj.Scroll4 = (float)scroll4.Value;
            obj.ShoreFalloff = (float)shoreFalloff.Value;
            obj.ShoreScale = (float)shoreScale.Value;
            obj.Speed = (float)speed.Value;
            obj.ReflectionFactorOffset = (float)reflectionFactorOffset.Value;            
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
