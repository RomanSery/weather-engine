using System.Windows;
using System.Windows.Input;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;
using WorldEditor.Controls;
using WorldEditor.Rendering;
using System.Windows.Media;
using System;
using SlimDX;

namespace WorldEditor.Global
{
    public class CustomWindow : Window
    {
        public string objCategory = null;
        public BaseGameObject objectToEdit = null;
        public ObjectWindow parentWindow = null;
        public event MouseEventHandler MouseMoving;
        public event MouseButtonEventHandler MouseRightBtnUp;


        #region Mouse events
        protected void view1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
                MainPlayer.Controller.LockCameraView = true;
        }

        protected void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseMoving != null)
                MouseMoving(sender, e);
        }

        protected void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseRightBtnUp != null)
                MouseRightBtnUp(sender, e);
        }


        protected void view1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
                MainPlayer.Controller.LockCameraView = false;

            D3DView view1 = (D3DView)sender;
            Point p = Mouse.GetPosition(view1);
            ObjectPicker.SelectionRay(e, p);
        }
        #endregion
    }
}
