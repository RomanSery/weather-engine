﻿#pragma checksum "..\..\..\..\Objects\EditObject_Material.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "DBA9C5A2BEB2D8A4ED6380E183C947B8"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using WorldEditor.Global;


namespace WorldEditor.Popups {
    
    
    /// <summary>
    /// EditObject_Material
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class EditObject_Material : WorldEditor.Global.CustomWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBaseObjectId;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBaseObjectName;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider specularPower;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider specularIntensity;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider Reflectivity;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider Emissive;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkRecieveRainSplashes;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkRecieveSnow;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkIsGlow;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkUseCubeMap;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnBrowse;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label cubeMapToLoad;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnOk;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\..\Objects\EditObject_Material.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/WorldEditor;component/objects/editobject_material.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Objects\EditObject_Material.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.txtBaseObjectId = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.txtBaseObjectName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.specularPower = ((System.Windows.Controls.Slider)(target));
            
            #line 32 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.specularPower.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.specularPower_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.specularIntensity = ((System.Windows.Controls.Slider)(target));
            
            #line 35 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.specularIntensity.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.specularIntensity_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Reflectivity = ((System.Windows.Controls.Slider)(target));
            
            #line 38 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.Reflectivity.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.Reflectivity_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Emissive = ((System.Windows.Controls.Slider)(target));
            
            #line 41 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.Emissive.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.Emissive_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.chkRecieveRainSplashes = ((System.Windows.Controls.CheckBox)(target));
            
            #line 44 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.chkRecieveRainSplashes.Click += new System.Windows.RoutedEventHandler(this.chkRecieveRainSplashes_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.chkRecieveSnow = ((System.Windows.Controls.CheckBox)(target));
            
            #line 45 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.chkRecieveSnow.Click += new System.Windows.RoutedEventHandler(this.chkRecieveSnow_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.chkIsGlow = ((System.Windows.Controls.CheckBox)(target));
            
            #line 46 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.chkIsGlow.Click += new System.Windows.RoutedEventHandler(this.chkIsGlow_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.chkUseCubeMap = ((System.Windows.Controls.CheckBox)(target));
            
            #line 47 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.chkUseCubeMap.Click += new System.Windows.RoutedEventHandler(this.chkUseCubeMap_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.btnBrowse = ((System.Windows.Controls.Button)(target));
            
            #line 51 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.btnBrowse.Click += new System.Windows.RoutedEventHandler(this.btnBrowseCubeMap_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.cubeMapToLoad = ((System.Windows.Controls.Label)(target));
            return;
            case 13:
            this.btnOk = ((System.Windows.Controls.Button)(target));
            
            #line 56 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.btnOk.Click += new System.Windows.RoutedEventHandler(this.btnOk_Click);
            
            #line default
            #line hidden
            return;
            case 14:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 57 "..\..\..\..\Objects\EditObject_Material.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnCancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
