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
using WeatherGame.Framework.Weather;
using WorldEditor.Global;

namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for TimePicker.xaml
    /// </summary>
    public partial class TimePicker : UserControl
    {
        public event EventHandler TimeChanged;
        private bool isClicked = false;
        private Point previousPoint = new Point(0, 0);
        private DateTime dt;


        public TimePicker()
        {
            InitializeComponent();
            dt = new DateTime(2006, 1, 1, 11, 0, 0);
            txtValue.Text = dt.ToShortTimeString();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CustomWindow w = (CustomWindow)Window.GetWindow(this);
            if (w != null)
            {
                w.MouseMoving += new MouseEventHandler(w_MouseMoving);
                w.MouseRightBtnUp += new MouseButtonEventHandler(w_MouseRightBtnUp);
            }          
        }



        void w_MouseMoving(object sender, MouseEventArgs e)
        {
            if (isClicked)
            {
                Mouse.SetCursor(Cursors.ScrollNS);
                float adjVal = 100.0f;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    adjVal = 300;
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    adjVal = 500;
                
                Point p = Mouse.GetPosition(btnScrubber);
                float n = (long)((previousPoint.Y - p.Y) * adjVal);
                dt = dt.AddSeconds(n);

                txtValue.Text = dt.ToShortTimeString();
                if (TimeChanged != null) TimeChanged(sender, e);
                previousPoint = p;
            }
        }
        void w_MouseRightBtnUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = false;
        }


        public DateTime SelectedTime
        {
            get
            {
                return dt;
            }
        }

        private void btnScrubber_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                isClicked = true;
                previousPoint = Mouse.GetPosition(btnScrubber);
            }
        }
        private void btnScrubber_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = false;
        }    
    }
}
