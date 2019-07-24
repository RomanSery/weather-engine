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
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Player;
using SlimDX.Direct3D10;

namespace WorldEditor.Popups
{
    /// <summary>
    /// Interaction logic for EditObject_GameObjectReference.xaml
    /// </summary>
    public partial class EditObject_GameObjectReference : CustomWindow
    {
        public GameObjectReference refToEdit = null;
        public Cell cellContainer = null;        

        public EditObject_GameObjectReference()
        {
            InitializeComponent();
            meshPos.PositionChanged += new EventHandler(meshPos_PositionChanged);
            meshRotation.RotationChanged += new EventHandler(meshRotation_RotationChanged);
            meshScale.ScaleChanged += new EventHandler(meshScale_ScaleChanged);
            spotLightTarget.PositionChanged += new EventHandler(spotLightTarget_PositionChanged);
            areaLightEndPoint.PositionChanged += new EventHandler(areaLightEndPoint_PositionChanged);
            boxLightDepth.NumericChanged += new EventHandler(boxLightDepth_NumericChanged);
            boxLightHeight.NumericChanged += new EventHandler(boxLightHeight_NumericChanged);
            boxLightWidth.NumericChanged += new EventHandler(boxLightWidth_NumericChanged);
        }

         

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            txtRefId.Text = refToEdit.RefId;
            txtRefName.Text = refToEdit.RefName;
            lblBaseObjectId.Content = "BaseObjectId: " + refToEdit.BaseObjectId + " (" + refToEdit.BaseObjectType + ")";

            meshPos.Pos = refToEdit.Position;
            meshRotation.RotationValues = refToEdit.RotationValues;
            meshScale.Scale = refToEdit.Scale;
            areaLightEndPoint.Pos = refToEdit.EndPoint;
            spotLightTarget.Pos = refToEdit.Target;
            lightIntensity.Value = refToEdit.Intensity;
            lightRadius.Value = refToEdit.MaxRange;            
            OuterAngle.Value = CameraHelper.ToDegrees(refToEdit.OuterAngle);
            InnerAngle.Value = CameraHelper.ToDegrees(refToEdit.InnerAngle);
            areaLightLerp.Value = refToEdit.LerpInc;

            boxLightWidth.NumericValue = refToEdit.BoxWidth;
            boxLightHeight.NumericValue = refToEdit.BoxHeight;
            boxLightDepth.NumericValue = refToEdit.BoxDepth;

            try
            {
                SlimDX.Vector3 c = refToEdit.LightColor;
                colorFill.Color = Color.FromArgb(255, Convert.ToByte(c.X * 255), Convert.ToByte(c.Y * 255), Convert.ToByte(c.Z * 255));
            }
            catch { }

            #region Animations
            if (refToEdit.AnimController != null && refToEdit.AnimController.Animations != null && refToEdit.AnimController.Animations.Count > 0)
            {
                animSpeed.Value = refToEdit.AnimController.Speed;
                int x = 0;
                foreach (Mesh3DAnimation anim in refToEdit.AnimController.Animations)
                {
                    
                    ComboBoxItem i = new ComboBoxItem();
                    i.Name = "Anim_" + x;
                    i.Content = anim.animName;
                    i.IsSelected = false;
                    ddlAnimations.Items.Add(i);
                    x++;                    
                }
            }
            #endregion

            #region Materials
            if (refToEdit.BaseGameObject is Model)
            {
                Model model = refToEdit.BaseGameObject as Model;
                subsetSelector.Items.Clear();
                renderSelectedSubsetOnly.IsChecked = false;

                MeshAttributeRange[] attr = model.Mesh3d.attrTable;
                foreach (MeshAttributeRange m in attr)
                {
                    ComboBoxItem i = new ComboBoxItem();
                    i.Name = "Subset_" + m.Id;
                    i.Content = m.Id.ToString() + "--" + refToEdit.getMaterialForSubset(m.Id).BaseObjectId;
                    if (subsetSelector.Items.Count == 0) i.IsSelected = true;
                    subsetSelector.Items.Add(i);
                }
            }

            List<BaseGameObject> wSettings = WorldData.GetObjectsByType(typeof(WaterSetting));
            foreach (WaterSetting ws in wSettings)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = ws.BaseObjectId;
                i.Content = ws.BaseObjectName;
                if (!string.IsNullOrEmpty(refToEdit.WaterSettingId) && ws.BaseObjectId == refToEdit.WaterSettingId) i.IsSelected = true;
                ddlWaterSetting.Items.Add(i);
            }     

            #endregion

            #region Attributes
            Array values = Enum.GetValues(typeof(GameObjectRefAttribute));
            foreach( GameObjectRefAttribute val in values )
            {
                CheckBox cb = new CheckBox();
                cb.Name = Enum.GetName(typeof(GameObjectRefAttribute), val);
                cb.Content = Enum.GetName(typeof(GameObjectRefAttribute), val);
                cb.IsChecked = refToEdit.getAttribute(val);
                cb.Click += new RoutedEventHandler(cb_Click);
                divAttributes.Children.Add(cb);
            }
            #endregion

            #region Scripts
            List<BaseGameObject> scripts = WorldData.GetObjectsByType(typeof(Script));
            foreach (BaseGameObject s in scripts)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = s.BaseObjectId;
                i.Content = s.BaseObjectName;
                i.IsSelected = refToEdit.UpdateScriptId != null && refToEdit.UpdateScriptId == s.BaseObjectId ? true : false;
                ddlUpdateScript.Items.Add(i);
            }

            if (refToEdit.updateScript != null && refToEdit.UpdateScriptArgs != null)
            {
                txtUpdateScriptArgs.Text = string.Join(",", refToEdit.UpdateScriptArgs);
            }
            #endregion

        }

        void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = ((CheckBox)sender);
            GameObjectRefAttribute r = (GameObjectRefAttribute)Enum.Parse(typeof(GameObjectRefAttribute), cb.Name);
            refToEdit.setAttribute(r, cb.IsChecked.HasValue && cb.IsChecked.Value);    
        }

        #region 3D data
        private void meshPos_PositionChanged(object sender, EventArgs e){ refToEdit.Position = meshPos.Pos;}
        void meshRotation_RotationChanged(object sender, EventArgs e)
        {            
            refToEdit.Rotation = meshRotation.Rotation;
            refToEdit.RotationValues = meshRotation.RotationValues;           
        }
        void meshScale_ScaleChanged(object sender, EventArgs e){ refToEdit.Scale = meshScale.Scale; }
        #endregion

        #region Lighting
        private void InnerAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { refToEdit.InnerAngle = CameraHelper.ToRadians((float)InnerAngle.Value); }
        private void OuterAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { refToEdit.OuterAngle = CameraHelper.ToRadians((float)OuterAngle.Value); }
        private void lightRadius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { refToEdit.MaxRange = (float)lightRadius.Value; }
        private void lightIntensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { refToEdit.Intensity = (float)lightIntensity.Value; }
        private void spotLightTarget_PositionChanged(object sender, EventArgs e) { refToEdit.Target = spotLightTarget.Pos;}
        private void areaLightEndPoint_PositionChanged(object sender, EventArgs e) { refToEdit.EndPoint = areaLightEndPoint.Pos; }
        private void areaLightLerp_ValueChanged(object sender, EventArgs e){ refToEdit.LerpInc = (float)areaLightLerp.Value; }
        private void ChangeLightColor(object sender, RoutedEventArgs e)
        {
            ColorPickerControl.ColorPickerDialog cPicker = new ColorPickerControl.ColorPickerDialog();
            cPicker.ColorChanging += new RoutedPropertyChangedEventHandler<Color>(cPicker_ColorChanging);


            SlimDX.Vector3 c = refToEdit.LightColor;
            cPicker.StartingColor = Color.FromArgb(255, Convert.ToByte(c.X * 255), Convert.ToByte(c.Y * 255), Convert.ToByte(c.Z * 255)); ;
            cPicker.Owner = Window.GetWindow(this);

            bool? dialogResult = cPicker.ShowDialog();
            if (dialogResult != null && (bool)dialogResult == true)
            {
                Color sc = cPicker.SelectedColor;
                colorFill.Color = sc;
                refToEdit.setColor(new Color3((float)(sc.R / 255.0), (float)(sc.G / 255.0), (float)(sc.B / 255.0)));

            }
        }
        private void cPicker_ColorChanging(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Color sc = e.NewValue;
            refToEdit.setColor(new Color3((float)(sc.R / 255.0), (float)(sc.G / 255.0), (float)(sc.B / 255.0)));
        }

        private void boxLightWidth_NumericChanged(object sender, EventArgs e) { refToEdit.BoxWidth = boxLightWidth.NumericValue; }        
        private void boxLightHeight_NumericChanged(object sender, EventArgs e) { refToEdit.BoxHeight = boxLightHeight.NumericValue; }                
        private void boxLightDepth_NumericChanged(object sender, EventArgs e) { refToEdit.BoxDepth = boxLightDepth.NumericValue; }                
        #endregion
        
        #region Animation
        private void ddlAnimations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (refToEdit.AnimController != null)
            {
                ComboBoxItem ci = ddlAnimations.SelectedItem as ComboBoxItem;
                if (ci != null)
                {
                    if (ci.Name == "Select")
                        refToEdit.AnimController.stopAnimation();
                    else
                        refToEdit.AnimController.playAnimation(ci.Content.ToString(), true);
                }                
            }
        }
        private void animSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (refToEdit.AnimController != null)
            {
                refToEdit.AnimController.Speed = (float)animSpeed.Value;                
            }
        }
        #endregion
        
        #region Materials
        private void renderSelectedSubsetOnly_Click(object sender, RoutedEventArgs e)
        {
            if (renderSelectedSubsetOnly.IsChecked.HasValue && renderSelectedSubsetOnly.IsChecked.Value)
            {
                ComboBoxItem ci = subsetSelector.SelectedItem as ComboBoxItem;
                if (ci != null)
                {
                    refToEdit.SelectedSubset = int.Parse(ci.Name.Replace("Subset_", ""));
                }
            }
            else
            {
                refToEdit.SelectedSubset = null;
            }
        }
        private void subsetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            materialSelector.Items.Clear();
            ComboBoxItem ci = subsetSelector.SelectedItem as ComboBoxItem;
            int subset = int.Parse(ci.Name.Replace("Subset_", ""));
            List<BaseGameObject> materials = WorldData.GetObjectsByType(typeof(Material));
            foreach (Material mat in materials)
            {
                ComboBoxItem i = new ComboBoxItem();
                i.Name = mat.BaseObjectId;
                i.Content = mat.BaseObjectName;
                if (mat.BaseObjectId == refToEdit.getMaterialForSubset(subset).BaseObjectId) i.IsSelected = true;
                materialSelector.Items.Add(i);
            }                       
        }
        private void materialSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            ComboBoxItem c = (ComboBoxItem)e.AddedItems[0];
            if(c != null)
            {
                ComboBoxItem ci = subsetSelector.SelectedItem as ComboBoxItem;
                int subset = int.Parse(ci.Name.Replace("Subset_", ""));

                Material mat = WorldData.GetObject(c.Name) as Material;
                refToEdit.setMaterialForSubset(subset, mat);

                ci.Content = subset + "--" + mat.BaseObjectId;
            }                       
        }
        #endregion


        private void ddlUpdateScript_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            ComboBoxItem c = (ComboBoxItem)e.AddedItems[0];
            if (c != null)
            {
                ComboBoxItem ci = ddlUpdateScript.SelectedItem as ComboBoxItem;
                Script script = WorldData.GetObject(ci.Name) as Script;
                if (script != null)
                {
                    lblArgsDesc.Content = "Args: " + script.ScriptArgsDesc;
                }
            }
        }
        private void ddlWaterSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            ComboBoxItem c = (ComboBoxItem)e.AddedItems[0];
            if (c != null)
            {
                ComboBoxItem ci = subsetSelector.SelectedItem as ComboBoxItem;
                refToEdit.WaterSettingId = ci.Name;
            }   
        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            
            refToEdit.RefName = txtRefName.Text;

            refToEdit.Position = meshPos.Pos;
            refToEdit.Rotation = meshRotation.Rotation;
            refToEdit.RotationValues = meshRotation.RotationValues;
            refToEdit.Scale = meshScale.Scale;
            refToEdit.EndPoint = areaLightEndPoint.Pos;
            refToEdit.Target = spotLightTarget.Pos;
            refToEdit.Intensity = (float)lightIntensity.Value;
            refToEdit.MaxRange = (float)lightRadius.Value;            
            refToEdit.InnerAngle = CameraHelper.ToRadians((float)InnerAngle.Value);
            refToEdit.OuterAngle = CameraHelper.ToRadians((float)OuterAngle.Value);
            refToEdit.LerpInc = (float)areaLightLerp.Value;

            refToEdit.BoxWidth = boxLightWidth.NumericValue;
            refToEdit.BoxHeight = boxLightHeight.NumericValue;
            refToEdit.BoxDepth = boxLightDepth.NumericValue;

            Color sc = colorFill.Color;
            refToEdit.setColor(new Color3((float)(sc.R / 255.0), (float)(sc.G / 255.0), (float)(sc.B / 255.0)));


            foreach (CheckBox cb in divAttributes.Children)
            {
                GameObjectRefAttribute r = (GameObjectRefAttribute)Enum.Parse(typeof(GameObjectRefAttribute), cb.Name);
                refToEdit.setAttribute(r, cb.IsChecked.HasValue && cb.IsChecked.Value);                
            }


            ComboBoxItem updateCi = ddlUpdateScript.SelectedItem as ComboBoxItem;
            if (updateCi != null)
            {
                if (updateCi.Content.ToString() == "Select")
                {
                    if (refToEdit.updateScript != null) refToEdit.updateScript.UnAssignScript(refToEdit);
                    refToEdit.updateScript = null;
                    refToEdit.UpdateScriptId = null;
                }
                else
                {
                    if (refToEdit.updateScript != null) refToEdit.updateScript.UnAssignScript(refToEdit);

                    Script newScript = WorldData.GetObject(updateCi.Name) as Script;

                    refToEdit.updateScript = (Script) Activator.CreateInstance(newScript.ClassName);
                    if (!string.IsNullOrEmpty(txtUpdateScriptArgs.Text))
                    {
                        refToEdit.UpdateScriptArgs = txtUpdateScriptArgs.Text.Split(',');
                    }
                    refToEdit.updateScript.AssignScript(refToEdit);
                    refToEdit.UpdateScriptId = newScript.BaseObjectId;
                }  
            }

            cellContainer.UpdateObjRef(refToEdit);
            
            this.Close();
            if (parentWindow != null) parentWindow.Refresh();           
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }









        

    }
}
