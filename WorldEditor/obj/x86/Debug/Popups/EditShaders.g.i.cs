﻿#pragma checksum "..\..\..\..\Popups\EditShaders.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "42B91864B0D14325BCFE126EA91ADC3C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
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
    /// EditShaders
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class EditShaders : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\..\Popups\EditShaders.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox shaders;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\..\Popups\EditShaders.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ICSharpCode.AvalonEdit.TextEditor scriptCode;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\..\Popups\EditShaders.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox lblErrMsg;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\..\Popups\EditShaders.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSaveScript;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\..\Popups\EditShaders.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCompileScript;
        
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
            System.Uri resourceLocater = new System.Uri("/WorldEditor;component/popups/editshaders.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Popups\EditShaders.xaml"
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
            
            #line 4 "..\..\..\..\Popups\EditShaders.xaml"
            ((WorldEditor.Popups.EditShaders)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.shaders = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.scriptCode = ((ICSharpCode.AvalonEdit.TextEditor)(target));
            return;
            case 4:
            this.lblErrMsg = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.btnSaveScript = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\..\..\Popups\EditShaders.xaml"
            this.btnSaveScript.Click += new System.Windows.RoutedEventHandler(this.btnSaveScript_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnCompileScript = ((System.Windows.Controls.Button)(target));
            
            #line 19 "..\..\..\..\Popups\EditShaders.xaml"
            this.btnCompileScript.Click += new System.Windows.RoutedEventHandler(this.btnCompileScript_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

