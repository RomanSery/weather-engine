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
    /// Interaction logic for EditObject_GlobalVar.xaml
    /// </summary>
    public partial class EditObject_Material : CustomWindow
    {
        private bool isEditingObj = false;
        private Material cancelRevert = null;

        public EditObject_Material()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Array values = Enum.GetValues(typeof(MaterialAttribute));
            foreach (MaterialAttribute val in values)
            {
                CheckBox cb = new CheckBox();
                cb.Name = Enum.GetName(typeof(MaterialAttribute), val);
                cb.Content = Enum.GetName(typeof(MaterialAttribute), val);                
                if (objectToEdit != null && objectToEdit is Material)
                {
                    cb.IsChecked = ((Material)objectToEdit).getAttribute(val);
                }
                cb.Click += new RoutedEventHandler(cb_Click);
                divAttributes.Children.Add(cb);
            }

            if (objectToEdit != null && objectToEdit is Material)
            {
                Material obj = objectToEdit as Material;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                txtMaterialIndex.Text = obj.MaterialIndex.ToString();
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;
                initValues(obj);
                cancelRevert = (Material)Utils.DeepClone(obj);                
            }
            else
            {
                Material m = new Material();
                m.MaterialIndex = WorldData.GetObjectsByType(typeof(Material)).Count;
                initValues(m);
            }
        }

        private void initValues(Material mObj)
        {
            specularPower.Value = mObj.SpecularPower;
            specularIntensity.Value = mObj.SpecularIntensity;
            Reflectivity.Value = mObj.Reflectivity;
            ReflectionSmoothness.Value = mObj.ReflectionSmoothness;
            Emissive.Value = mObj.Emissive;
            cubeMapToLoad.Content = mObj.CubeMapTextureName;
            detailMapToLoad.Content = mObj.DetailMapTextureName;            
            txtMaterialIndex.Text = mObj.MaterialIndex.ToString();

            roughness.Value = mObj.Roughness;
            refAtNormIncidence.Value = mObj.RefAtNormIncidence;
            smoothness.Value = mObj.Smoothness;
            metalness.Value = mObj.Metalness;

            anisotropicRoughnessX.Value = mObj.AnisotropicRoughnessX;
            anisotropicRoughnessY.Value = mObj.AnisotropicRoughnessY;

            string lm = mObj.LightModel.ToString();
            foreach (ComboBoxItem cbItem in ddlLightingModel.Items)
            {
                if (cbItem.Name == lm)
                {
                    cbItem.IsSelected = true;
                    break;
                }
            }   
        }

        void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = ((CheckBox)sender);
            MaterialAttribute r = (MaterialAttribute)Enum.Parse(typeof(MaterialAttribute), cb.Name);
            if (objectToEdit != null && objectToEdit is Material)
            {
                ((Material)objectToEdit).setAttribute(r, cb.IsChecked.HasValue && cb.IsChecked.Value);
            }
        }

                
        private void specularPower_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).SpecularPower = (int)e.NewValue;
        }
        private void specularIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).SpecularIntensity = (float)e.NewValue;
        }
        private void Reflectivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).Reflectivity = (float)e.NewValue;
        }
        private void ReflectionSmoothness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).ReflectionSmoothness = (float)e.NewValue;
        }
        private void Emissive_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).Emissive = (float)e.NewValue;
        }


        #region Lighting model
        private void ddlLightingModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ComboBoxItem cbItem = ddlLightingModel.SelectedItem as ComboBoxItem;
                ((Material)objectToEdit).LightModel = (LightingModel) Enum.Parse(typeof(LightingModel), cbItem.Name.ToString());
            }
        }

        private void roughness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).Roughness = (float)e.NewValue;
        }
        private void refAtNormIncidence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).RefAtNormIncidence = (float)e.NewValue;
        }
        private void smoothness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).Smoothness = (float)e.NewValue;
        }
        private void metalness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).Metalness = (float)e.NewValue;
        }

        private void anisotropicRoughnessX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).AnisotropicRoughnessX = (float)e.NewValue;
        }

        private void anisotropicRoughnessY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (objectToEdit != null) ((Material)objectToEdit).AnisotropicRoughnessY = (float)e.NewValue;
        }
        #endregion



        #region texture maps
        private void btnBrowseCubeMap_Click(object sender, RoutedEventArgs e)
        {
            string s = Utils.GetRootPath() + "\\Content\\Textures\\fx";

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = s;
            dlg.Filter = "DDS (.dds)|*.dds"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string texName = System.IO.Path.GetFileName(dlg.FileName);
                cubeMapToLoad.Content = texName;
            }
        }
        private void btnRemoveCubeMap_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ((Material)objectToEdit).CubeMapSRV = null;
                ((Material)objectToEdit).CubeMapTextureName = null;
                cubeMapToLoad.Content = "";
            }
        }


        private void btnBrowseDetailMap_Click(object sender, RoutedEventArgs e)
        {
            string s = Utils.GetRootPath() + "\\Content\\Textures\\detail";

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = s;
            dlg.Filter = "DDS (.dds)|*.dds"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string texName = System.IO.Path.GetFileName(dlg.FileName);
                detailMapToLoad.Content = texName;
            }
        }
        private void btnRemoveDetailMap_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ((Material)objectToEdit).DetailMapSRV = null;
                ((Material)objectToEdit).DetailMapTextureName = null;
                detailMapToLoad.Content = "";
            }
        }
        #endregion

            


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new Material();

            Material obj = objectToEdit as Material;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;
            obj.MaterialIndex = int.Parse(txtMaterialIndex.Text);

            //custom code
            obj.Emissive = (float)Emissive.Value;
            obj.Reflectivity = (float)Reflectivity.Value;
            obj.ReflectionSmoothness = (float)ReflectionSmoothness.Value;
            obj.SpecularIntensity = (float)specularIntensity.Value;
            obj.SpecularPower = (int)specularPower.Value;

            obj.Roughness = (float)roughness.Value;
            obj.RefAtNormIncidence = (float)refAtNormIncidence.Value;
            obj.Smoothness = (float)smoothness.Value;
            obj.Metalness = (float)metalness.Value;

            obj.AnisotropicRoughnessX = (float)anisotropicRoughnessX.Value;
            obj.AnisotropicRoughnessY = (float)anisotropicRoughnessY.Value;

            if (ddlLightingModel.SelectedItem != null)
            {
                obj.LightModel = (LightingModel)Enum.Parse(typeof(LightingModel), ((ComboBoxItem)ddlLightingModel.SelectedItem).Name);
            }

            foreach (CheckBox cb in divAttributes.Children)
            {
                MaterialAttribute r = (MaterialAttribute)Enum.Parse(typeof(MaterialAttribute), cb.Name);
                obj.setAttribute(r, cb.IsChecked.HasValue && cb.IsChecked.Value);
            }            

            String cubTexName = cubeMapToLoad.Content != null ? cubeMapToLoad.Content.ToString() : "";
            if (string.IsNullOrEmpty(cubTexName))
            {
                obj.CubeMapSRV = null;
                obj.CubeMapTextureName = null;
            }
            else
            {
                obj.CubeMapSRV = TextureLoader.LoadCubeTexture("fx/" + cubTexName);
                obj.CubeMapTextureName = cubTexName;                
            }

            String detailTexName = detailMapToLoad.Content != null ? detailMapToLoad.Content.ToString() : "";
            if (string.IsNullOrEmpty(detailTexName))
            {
                obj.DetailMapSRV = null;
                obj.DetailMapTextureName = null;
            }
            else
            {
                obj.DetailMapSRV = TextureLoader.LoadTexture("detail/" + detailTexName);
                obj.DetailMapTextureName = detailTexName;
            }



            
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
