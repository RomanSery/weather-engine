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
using WeatherGame.Framework.Scripting;
using System.Reflection;

namespace WorldEditor.Popups
{
    /// <summary>
    /// Interaction logic for EditObject_Script.xaml
    /// </summary>
    public partial class EditObject_Script : CustomWindow
    {
        private bool isEditingObj = false;

        public EditObject_Script()
        {
            InitializeComponent();      
        }     

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is Script)
            {
                Script obj = objectToEdit as Script;
                txtBaseObjectId.Text = obj.BaseObjectId;
                txtBaseObjectName.Text = obj.BaseObjectName;

                scriptCode.Text = obj.ScriptText;
                txtArgsDesc.Text = obj.ScriptArgsDesc;

                isEditingObj = true;
                txtBaseObjectId.IsEnabled = false;
            }
            else
            {
                scriptCode.Text = ScriptManager.scriptTemplate;
                btnCompileScript.Visibility = System.Windows.Visibility.Hidden;
            }

        }

        private void btnCompileScript_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit != null && objectToEdit is Script)
            {
                string errMsg = string.Empty;
                Type className = null;
                bool result = ScriptManager.CompileScript(scriptCode.Text, objectToEdit.BaseObjectId, out errMsg, true, out className);
                lblErrMsg.Text = errMsg;

                ((Script)objectToEdit).ClassName = className;
            }
        }


        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (objectToEdit == null) objectToEdit = new Script();

            Script obj = objectToEdit as Script;
            obj.BaseObjectId = txtBaseObjectId.Text;
            obj.BaseObjectName = txtBaseObjectName.Text;
            obj.setScriptText(scriptCode.Text);
            obj.ScriptArgsDesc = txtArgsDesc.Text;

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
