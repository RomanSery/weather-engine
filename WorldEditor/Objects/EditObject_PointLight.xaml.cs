using System;
using System.Windows;
using System.Windows.Media;
using SlimDX;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.World;
using WorldEditor.Global;
using WeatherGame.Framework.Utilities;

namespace WorldEditor.Popups
{
    /// <summary>
    /// Interaction logic for EditObject_PointLight.xaml
    /// </summary>
    public partial class EditObject_PointLight : CustomWindow
    {
        private bool isEditingObj = false;
        private PointLight cancelRevert = null;

        public EditObject_PointLight()
        {
            InitializeComponent();            
        }
           

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is PointLight)
            {
                PointLight obj = objectToEdit as PointLight;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;                                
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                //custom code                                
                haloToLoad.Content = obj.HaloTextureName;
                maskToLoad.Content = obj.LightMaskTextureName;
                mapToLoad.Content = obj.LightMapTextureName;

                cancelRevert = (PointLight)Utils.DeepClone(obj); 
                
                //custom code end
            }           
        }


        private void btnRemoveHalo_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ((PointLight)objectToEdit).HaloSRV = null;
                ((PointLight)objectToEdit).HaloTextureName = null;
                haloToLoad.Content = "";
            }
        }

        private void btnRemoveMask_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ((PointLight)objectToEdit).LightMaskSRV = null;
                ((PointLight)objectToEdit).LightMaskTextureName = null;
                maskToLoad.Content = "";
            }
        }

        private void btnRemoveMap_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null)
            {
                ((PointLight)objectToEdit).LightMapSRV = null;
                ((PointLight)objectToEdit).LightMapTextureName = null;
                mapToLoad.Content = "";
            }
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new PointLight();

            PointLight obj = objectToEdit as PointLight;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;

            //custom code
            String haloTexName = haloToLoad.Content != null ? haloToLoad.Content.ToString() : "";
            if (string.IsNullOrEmpty(haloTexName))
            {
                obj.HaloSRV = null;
                obj.HaloTextureName = null;
            }
            else
            {
                obj.HaloSRV = TextureLoader.LoadTexture("flares/" + haloTexName);
                obj.HaloTextureName = haloTexName;
            }

            String maskTexName = maskToLoad.Content != null ? maskToLoad.Content.ToString() : "";
            if (string.IsNullOrEmpty(maskTexName))
            {
                obj.LightMaskSRV = null;
                obj.LightMaskTextureName = null;
            }
            else
            {
                obj.LightMaskSRV = TextureLoader.LoadCubeTexture("fx/" + maskTexName);
                obj.LightMaskTextureName = maskTexName;
            }

            String mapTexName = mapToLoad.Content != null ? mapToLoad.Content.ToString() : "";
            if (string.IsNullOrEmpty(mapTexName))
            {
                obj.LightMapSRV = null;
                obj.LightMapTextureName = null;
            }
            else
            {
                obj.LightMapSRV = TextureLoader.LoadCubeTexture("fx/" + mapTexName);
                obj.LightMapTextureName = mapTexName;
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



        private void btnBrowseHalo_Click(object sender, RoutedEventArgs e)
        {
            string s = Utils.GetRootPath() + "\\Content\\Textures\\flares";            

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = s;
            dlg.Filter = "PNG (.png)|light*.png"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {               
                string texName = System.IO.Path.GetFileName(dlg.FileName);
                haloToLoad.Content = texName;
                if (objectToEdit != null)
                {
                    ((PointLight)objectToEdit).HaloSRV = TextureLoader.LoadTexture("flares/" + texName);
                    ((PointLight)objectToEdit).HaloTextureName = texName;                 
                }
            }
        }
        private void btnBrowseMask_Click(object sender, RoutedEventArgs e)
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
                maskToLoad.Content = texName;
                if (objectToEdit != null)
                {
                    ((PointLight)objectToEdit).LightMaskSRV = TextureLoader.LoadCubeTexture("fx/" + texName);
                    ((PointLight)objectToEdit).LightMaskTextureName = texName;                    
                }
            }
        }
        private void btnBrowseMap_Click(object sender, RoutedEventArgs e)
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
                mapToLoad.Content = texName;
                if (objectToEdit != null)
                {
                    ((PointLight)objectToEdit).LightMapSRV = TextureLoader.LoadCubeTexture("fx/" + texName);
                    ((PointLight)objectToEdit).LightMapTextureName = texName;                    
                }
            }
        }

       

        

    }
}
