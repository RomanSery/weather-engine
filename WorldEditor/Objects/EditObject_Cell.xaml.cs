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
    /// Interaction logic for EditObject_Cell.xaml
    /// </summary>
    public partial class EditObject_Cell : CustomWindow
    {
        private bool isEditingObj = false;

        public EditObject_Cell()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            foreach (WorldSpace w in worldSpaces)
            {
                ComboBoxItem i = new ComboBoxItem();                
                i.Name = w.BaseObjectId;
                i.Content = w.BaseObjectId;
                i.IsSelected = false;
                ddlWorld.Items.Add(i); 
            }

            if (objectToEdit != null && objectToEdit is Cell)
            {
                Cell obj = objectToEdit as Cell;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;
                txtPosition.Text = obj.CellPos.X + "," + obj.CellPos.Y;                 
                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;

                if (!string.IsNullOrEmpty(obj.WorldSpaceId)) { foreach (ComboBoxItem ci in ddlWorld.Items) if (ci.Name == obj.WorldSpaceId) { ci.IsSelected = true; break; } }                                    
            }           
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new Cell();

            Cell obj = objectToEdit as Cell;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;           

            try
            {
                string[] s = txtPosition.Text.Split(',');
                Vector2 pos = new Vector2(int.Parse(s[0]), int.Parse(s[1]));
                obj.CellPos = pos;                
            }
            catch
            {
                MessageBox.Show(Application.Current.MainWindow, "Invalid cell pos", "Invalid cell pos", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Cancel);
                return;
            }

            if (ddlWorld.SelectedItem == null)
            {
                MessageBox.Show(Application.Current.MainWindow, "Select world", "Select world", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Cancel);
                return;
            }

            obj.WorldSpaceId = ((ComboBoxItem)ddlWorld.SelectedItem).Name;

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



        

    }
}
