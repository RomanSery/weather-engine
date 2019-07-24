﻿#pragma checksum "..\..\..\..\Popups\Effects.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8B132D8A203AC877AD7AF7E6B04DEFC4"
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


namespace WorldEditor.Popups {
    
    
    /// <summary>
    /// Popup_Effects
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class Popup_Effects : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Expander bloomSettings;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox isBloomOn;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BlurAmount;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BloomThreshold;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BloomIntensity;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BaseIntensity;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BloomSaturation;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\Popups\Effects.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BaseSaturation;
        
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
            System.Uri resourceLocater = new System.Uri("/WorldEditor;component/popups/effects.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Popups\Effects.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
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
            
            #line 4 "..\..\..\..\Popups\Effects.xaml"
            ((WorldEditor.Popups.Popup_Effects)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.bloomSettings = ((System.Windows.Controls.Expander)(target));
            return;
            case 3:
            this.isBloomOn = ((System.Windows.Controls.CheckBox)(target));
            
            #line 24 "..\..\..\..\Popups\Effects.xaml"
            this.isBloomOn.Click += new System.Windows.RoutedEventHandler(this.isBloomOn_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BlurAmount = ((System.Windows.Controls.Slider)(target));
            
            #line 27 "..\..\..\..\Popups\Effects.xaml"
            this.BlurAmount.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.bloom_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.BloomThreshold = ((System.Windows.Controls.Slider)(target));
            
            #line 30 "..\..\..\..\Popups\Effects.xaml"
            this.BloomThreshold.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.bloom_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.BloomIntensity = ((System.Windows.Controls.Slider)(target));
            
            #line 33 "..\..\..\..\Popups\Effects.xaml"
            this.BloomIntensity.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.bloom_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.BaseIntensity = ((System.Windows.Controls.Slider)(target));
            
            #line 36 "..\..\..\..\Popups\Effects.xaml"
            this.BaseIntensity.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.bloom_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.BloomSaturation = ((System.Windows.Controls.Slider)(target));
            
            #line 39 "..\..\..\..\Popups\Effects.xaml"
            this.BloomSaturation.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.bloom_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.BaseSaturation = ((System.Windows.Controls.Slider)(target));
            
            #line 42 "..\..\..\..\Popups\Effects.xaml"
            this.BaseSaturation.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.bloom_ValueChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

