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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorldEditor.Rendering;
using System.Reflection;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using D3D10 = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;

using WeatherGame.Framework;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.Utilities;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Player;
using WeatherGame.RenderLoop;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.World;
using WorldEditor.Global;



namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for ObjectWindow.xaml
    /// </summary>
    public partial class ObjectWindow : Window
    {
        private Point startPoint;

        public ObjectWindow()
        {            
            InitializeComponent();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
                  
        }

        #region objectTypes
        private void objectTypes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = objectTypes.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag != null)
            {
                objectTypes.ContextMenu = objectTypes.Resources["addContext"] as System.Windows.Controls.ContextMenu;

                objectsGrid.Items.Clear();
                Assembly a = Assembly.GetAssembly(typeof(GlobalVar));
                Type objType = a.GetType("WeatherGame.Framework.Objects." + selectedItem.Tag);

                List<BaseGameObject> objects = WorldData.GetObjectsByType(objType);
                foreach (BaseGameObject obj in objects)
                {
                    if (obj.Category != null && obj.Category != selectedItem.Header.ToString()) continue;
                    objectsGrid.Items.Add(obj);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            TreeViewItem selectedItem = objectTypes.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Tag != null)
            {
                var type = Type.GetType("WorldEditor.Popups.EditObject_" + selectedItem.Tag);
                CustomWindow objectNewWindow = (CustomWindow)Activator.CreateInstance(type);
                objectNewWindow.Owner = this.Owner;                
                objectNewWindow.objCategory = selectedItem.Header.ToString();
                objectNewWindow.parentWindow = this;
                objectNewWindow.Show();
            }

        }
        #endregion


        #region objectsGrid
        private void objectsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BaseGameObject selectedItem = (BaseGameObject)objectsGrid.SelectedItem;
            if (selectedItem != null)
            {
                objectsGrid.ContextMenu = objectsGrid.Resources["deleteContext"] as System.Windows.Controls.ContextMenu;
            }
        }

        private void objectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BaseGameObject obj = (BaseGameObject)objectsGrid.SelectedItem;

            var type = Type.GetType("WorldEditor.Popups.EditObject_" + obj.GetType().Name);
            CustomWindow objectNewWindow = (CustomWindow)Activator.CreateInstance(type);
            objectNewWindow.Owner = this.Owner;
            objectNewWindow.objectToEdit = obj;

            TreeViewItem selectedItem = objectTypes.SelectedItem as TreeViewItem;
            if (selectedItem != null) objectNewWindow.objCategory = selectedItem.Header.ToString();

            objectNewWindow.parentWindow = this;
            objectNewWindow.Show();
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            BaseGameObject obj = (BaseGameObject)objectsGrid.SelectedItem;
            if (obj == null) return;

            WorldData.DeleteObject(obj);

            Refresh();
        }

        private void objectsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void objectsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                if (objectsGrid.SelectedItem == null) return;
                BaseGameObject obj = (BaseGameObject)objectsGrid.SelectedItem;
                if (obj == null) return;

                DataGridRow container = (DataGridRow)objectsGrid.ItemContainerGenerator.ContainerFromItem(objectsGrid.SelectedItem);
                DataObject dataObj = new DataObject("GameObjectReference", obj);
                DragDrop.DoDragDrop(container, dataObj, DragDropEffects.Move);
            }
        }
        #endregion

                      

     
        public void Refresh()
        {
            objectTypes_SelectedItemChanged(this, null);
        }

        private void btnSaveData_Click(object sender, RoutedEventArgs e)
        {
            WorldData.SaveData();
            MessageBox.Show(Application.Current.MainWindow, "Saved", "Saved", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Cancel);
        }     


    }
}
