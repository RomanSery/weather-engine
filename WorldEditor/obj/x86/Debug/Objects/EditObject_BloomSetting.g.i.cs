﻿#pragma checksum "..\..\..\..\Objects\EditObject_BloomSetting.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5800D07260B25CAEF9BE44A678A66B52"
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
    /// EditObject_BloomSetting
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class EditObject_BloomSetting : WorldEditor.Global.CustomWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBaseObjectId;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBaseObjectName;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BlurAmount;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BloomThreshold;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BloomIntensity;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BaseIntensity;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BloomSaturation;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider BaseSaturation;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnOk;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
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
            System.Uri resourceLocater = new System.Uri("/WorldEditor;component/objects/editobject_bloomsetting.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
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
            this.BlurAmount = ((System.Windows.Controls.Slider)(target));
            return;
            case 4:
            this.BloomThreshold = ((System.Windows.Controls.Slider)(target));
            return;
            case 5:
            this.BloomIntensity = ((System.Windows.Controls.Slider)(target));
            return;
            case 6:
            this.BaseIntensity = ((System.Windows.Controls.Slider)(target));
            return;
            case 7:
            this.BloomSaturation = ((System.Windows.Controls.Slider)(target));
            return;
            case 8:
            this.BaseSaturation = ((System.Windows.Controls.Slider)(target));
            return;
            case 9:
            this.btnOk = ((System.Windows.Controls.Button)(target));
            
            #line 52 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
            this.btnOk.Click += new System.Windows.RoutedEventHandler(this.btnOk_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\..\Objects\EditObject_BloomSetting.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnCancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

