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
using WeatherGame.Framework.Mesh3d;
using SlimDX.Direct3D10;
using System.Reflection;

namespace WorldEditor.Popups
{
    /// <summary>
    /// Interaction logic for EditObject_Model.xaml
    /// </summary>
    public partial class EditObject_Model : CustomWindow
    {
        private bool isEditingObj = false;

        public EditObject_Model()
        {
            InitializeComponent();            
            mass.MinValue = 1;
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is Model)
            {
                Model obj = objectToEdit as Model;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                meshToLoad.Content = obj.Mesh3d.meshFileName.Replace(".mesh", ".X");
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;
                isStaticMesh.IsChecked = obj.StaticMesh;
                mass.NumericValue = obj.Mass;

                foreach (ComboBoxItem cbItem in ddlInputLayout.Items)
                {
                    FieldInfo myf = typeof(MeshInputElements10).GetField(cbItem.Content.ToString());
                    InputElement[] layout = (InputElement[])myf.GetValue(null);
                    if (layout == obj.Mesh3d.inputElements)
                    {
                        cbItem.IsSelected = true;
                        break;
                    }
                }                
            }           
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            

            if (meshToLoad.Content == null || string.IsNullOrEmpty(meshToLoad.Content.ToString()))
            {
                MessageBox.Show("Select the mesh to load");
                return;
            }

            ComboBoxItem cbItem = ddlInputLayout.SelectedItem as ComboBoxItem;
            //FieldInfo myf = typeof(MeshInputElements10).GetField(cbItem.Content.ToString());
            //InputElement[] layout = (InputElement[])myf.GetValue(null);

            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Replace("WorldEditor", "BuildContent") + "\\WeatherGame.BuildContent.exe";
            path = path.Replace("file:\\", "");
            string outputFile = meshToLoad.Content.ToString().Replace(".X", ".mesh");           
            ProcessStartInfo s = new ProcessStartInfo();
            s.Arguments = meshToLoad.Content.ToString() + "," + cbItem.Content.ToString();
            s.FileName = path;
            s.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            using (Process p = Process.Start(s))
            {
                p.WaitForExit();
            }

            



            if (objectToEdit == null) objectToEdit = new Model();
            Model obj = objectToEdit as Model;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;                     
            obj.FileName = outputFile;
            obj.Layout = cbItem.Content.ToString();
            obj.StaticMesh = (isStaticMesh.IsChecked.HasValue && isStaticMesh.IsChecked.Value) ? true : false;
            obj.Mass = mass.NumericValue;

            if (isEditingObj)
            {
                obj.UpdateMesh();
            }

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
            this.Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {            
            string s = Utils.GetRootPath() + "\\Content\\export";

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = s;
            dlg.Filter = "Xfile (.X)|*.X"; // Filter files by extension            

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {                
                //meshToLoad.Content = System.IO.Path.GetFileName(dlg.FileName);
                meshToLoad.Content = dlg.FileName.Replace(s + "\\", "");
            }
        }        


        

    }
}
