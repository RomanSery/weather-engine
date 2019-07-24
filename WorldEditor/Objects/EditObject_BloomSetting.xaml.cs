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
    /// Interaction logic for EditObject_BloomSetting.xaml
    /// </summary>
    public partial class EditObject_BloomSetting : CustomWindow
    {
        private bool isEditingObj = false;
        private BloomSetting cancelRevert = null;

        public EditObject_BloomSetting()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is BloomSetting)
            {
                BloomSetting obj = objectToEdit as BloomSetting;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                initValues(obj);
                cancelRevert = (BloomSetting)Utils.DeepClone(obj);                
            }
            else
            {
                BloomSetting bs = new BloomSetting();                
                initValues(bs);
            }
        }

        private void initValues(BloomSetting mObj)
        {
            BlurAmount.Value = mObj.BlurAmount;
            BloomThreshold.Value = mObj.BloomThreshold;
            BloomIntensity.Value = mObj.BloomIntensity;
            BaseIntensity.Value = mObj.BaseIntensity;
            BloomSaturation.Value = mObj.BloomSaturation;
            BaseSaturation.Value = mObj.BaseSaturation;
        }


        private void BlurAmount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((BloomSetting)objectToEdit).BlurAmount = (float)e.NewValue;
        }
        private void BloomThreshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((BloomSetting)objectToEdit).BloomThreshold = (float)e.NewValue;
        }
        private void BloomIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((BloomSetting)objectToEdit).BloomIntensity = (float)e.NewValue;
        }
        private void BaseIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((BloomSetting)objectToEdit).BaseIntensity = (float)e.NewValue;
        }
        private void BloomSaturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((BloomSetting)objectToEdit).BloomSaturation = (float)e.NewValue;
        }
        private void BaseSaturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((BloomSetting)objectToEdit).BaseSaturation = (float)e.NewValue;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new BloomSetting();

            BloomSetting obj = objectToEdit as BloomSetting;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            //custom code            
            obj.BlurAmount = (float)BlurAmount.Value;
            obj.BloomThreshold = (float)BloomThreshold.Value;
            obj.BloomIntensity = (float)BloomIntensity.Value;
            obj.BaseIntensity = (float)BaseIntensity.Value;
            obj.BloomSaturation = (float)BloomSaturation.Value;
            obj.BaseSaturation = (float)BaseSaturation.Value;
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
