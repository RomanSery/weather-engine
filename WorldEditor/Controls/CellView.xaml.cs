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
using WorldEditor.Popups;
using WeatherGame.Framework.physics;



namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for CellView.xaml
    /// </summary>
    public partial class CellView : Window
    {
        public CellView()
        {            
            InitializeComponent();

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<BaseGameObject> worldSpaces = WorldData.GetObjectsByType(typeof(WorldSpace));
            int x = 0;
            foreach (WorldSpace w in worldSpaces)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = w.BaseObjectId;
                i.Content = w.BaseObjectId;
                i.IsSelected = x == 0 ? true: false;
                ddlWorld.Items.Add(i);
                x++;
            }
            ddlWorld_SelectionChanged(sender, null);
        }

        #region cells
        private void ddlWorld_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cellsGrid.Items.Clear();
            objectsGrid.Items.Clear();

            ComboBoxItem ci = ddlWorld.SelectedItem as ComboBoxItem;
            if (ci != null)
            {
                WorldSpace ws = WorldData.GetObject(ci.Name) as WorldSpace;
                foreach (Cell cell in ws.AllCells) cellsGrid.Items.Add(cell);
            }
        }

        private void cellsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            objectsGrid.Items.Clear();
            Cell obj = cellsGrid.SelectedItem as Cell;
            if (obj == null) return;

            GlobalSettings.CurrentCell = obj;

            Dictionary<String, GameObjectReference> objRefs = obj.CellObjectRefs;
            if (objRefs != null)
            {
                foreach (GameObjectReference objRef in objRefs.Values) objectsGrid.Items.Add(objRef);
            }
        }
        #endregion

        

        private void objectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {            
            GameObjectReference objRef = (GameObjectReference)objectsGrid.SelectedItem;
            Cell cellObj = cellsGrid.SelectedItem as Cell;

            EditObject_GameObjectReference objRefWindow = new EditObject_GameObjectReference();
            objRefWindow.Owner = this.Owner;
            objRefWindow.cellContainer = cellObj;
            objRefWindow.refToEdit = objRef;
            //objRefWindow.parentWindow = this;
            objRefWindow.Show();
        }

        private void MenuItem_DeleteClick(object sender, RoutedEventArgs e)
        {
            GameObjectReference objRef = (GameObjectReference)objectsGrid.SelectedItem;
            if (objRef == null) return;

            Cell cellObj = cellsGrid.SelectedItem as Cell;
            if (cellObj == null) return;

            cellObj.DeleteObjRef(objRef.RefId);


            cellsGrid_MouseDoubleClick(sender, null);
        }


        private void DropList_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("GameObjectReference") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }
        private void DropList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("GameObjectReference"))
            {
                BaseGameObject baseGameObj = e.Data.GetData("GameObjectReference") as BaseGameObject;
                if (baseGameObj.CanHaveRefs)
                {
                    Cell cellObj = cellsGrid.SelectedItem as Cell;
                    if (cellObj == null) return;

                    cellObj.AddObjRef(baseGameObj.BaseObjectId);                                        

                    cellsGrid_MouseDoubleClick(sender, null);
                }
                
            }
        }

        private void objectsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GameObjectReference objRef = (GameObjectReference)objectsGrid.SelectedItem;
            if (objRef != null)
            {
                objectsGrid.ContextMenu = objectsGrid.Resources["deleteContext"] as System.Windows.Controls.ContextMenu;
                GlobalSettings.CurrentObjRef = objRef;
            }
            else
                GlobalSettings.CurrentObjRef = null;
            
        }

       
        
    }
}
