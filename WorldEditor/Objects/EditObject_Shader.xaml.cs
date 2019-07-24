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
    /// Interaction logic for EditObject_Shader.xaml
    /// </summary>
    public partial class EditObject_Shader : CustomWindow
    {
        private bool isEditingObj = false;

        public EditObject_Shader()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is Shader)
            {
                Shader obj = objectToEdit as Shader;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                shaderCode.Text = obj.ShaderString;                
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;
            }           
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            lblErrMsg.Text = "";
            if (objectToEdit == null) objectToEdit = new Shader();

            Shader obj = objectToEdit as Shader;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;
            obj.ShaderString = shaderCode.Text;

            string err = obj.Recompile();
            if (!string.IsNullOrEmpty(err))
            {
                lblErrMsg.Text = err;
                return;
            }
            else
            {
                lblErrMsg.Text = "";
                DeferredRenderer.InitializeShaderMaterials();
            }

            obj.Category = objCategory;
            bool success = isEditingObj ? WorldData.UpdateObject(obj) : WorldData.AddObject(obj);
            if (!success)
                MessageBox.Show(Application.Current.MainWindow, "ID already exists", "AddObject Failure", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Cancel);
            else
            {
                //this.Close();
                //if (parentWindow != null) parentWindow.Refresh();
            }


        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        

    }
}
