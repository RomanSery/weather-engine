﻿#pragma checksum "..\..\..\Controls\RenderingOptions.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "204D43C509A8FAB7E533919292945483"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
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
using WorldEditor.Controls;


namespace WorldEditor.Controls {
    
    
    /// <summary>
    /// RenderingOptions
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class RenderingOptions : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkRenderLights;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkRenderGrid;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkRenderVolume;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkPhysics;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox metersPerSquare;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkPhysicsCamera;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.SolidColorBrush colorFill;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal WorldEditor.Controls.NumericSpinner gridPos;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Controls\RenderingOptions.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider cameraSpeed;
        
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
            System.Uri resourceLocater = new System.Uri("/WorldEditor;component/controls/renderingoptions.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\RenderingOptions.xaml"
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
            this.chkRenderLights = ((System.Windows.Controls.CheckBox)(target));
            
            #line 13 "..\..\..\Controls\RenderingOptions.xaml"
            this.chkRenderLights.Click += new System.Windows.RoutedEventHandler(this.chkRenderLights_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.chkRenderGrid = ((System.Windows.Controls.CheckBox)(target));
            
            #line 14 "..\..\..\Controls\RenderingOptions.xaml"
            this.chkRenderGrid.Click += new System.Windows.RoutedEventHandler(this.chkRenderGrid_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.chkRenderVolume = ((System.Windows.Controls.CheckBox)(target));
            
            #line 15 "..\..\..\Controls\RenderingOptions.xaml"
            this.chkRenderVolume.Click += new System.Windows.RoutedEventHandler(this.chkRenderVolume_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.chkPhysics = ((System.Windows.Controls.CheckBox)(target));
            
            #line 16 "..\..\..\Controls\RenderingOptions.xaml"
            this.chkPhysics.Click += new System.Windows.RoutedEventHandler(this.chkPhysics_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.metersPerSquare = ((System.Windows.Controls.ComboBox)(target));
            
            #line 17 "..\..\..\Controls\RenderingOptions.xaml"
            this.metersPerSquare.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.metersPerSquare_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.chkPhysicsCamera = ((System.Windows.Controls.CheckBox)(target));
            
            #line 25 "..\..\..\Controls\RenderingOptions.xaml"
            this.chkPhysicsCamera.Click += new System.Windows.RoutedEventHandler(this.chkPhysicsCamera_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 31 "..\..\..\Controls\RenderingOptions.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ChangeLightColor);
            
            #line default
            #line hidden
            return;
            case 8:
            this.colorFill = ((System.Windows.Media.SolidColorBrush)(target));
            return;
            case 9:
            this.gridPos = ((WorldEditor.Controls.NumericSpinner)(target));
            return;
            case 10:
            this.cameraSpeed = ((System.Windows.Controls.Slider)(target));
            
            #line 39 "..\..\..\Controls\RenderingOptions.xaml"
            this.cameraSpeed.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.cameraSpeed_ValueChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
