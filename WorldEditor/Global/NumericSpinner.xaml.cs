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
using WorldEditor.Global;

namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for NumericSpinner.xaml
    /// </summary>
    public partial class NumericSpinner : UserControl
    {
        public event EventHandler NumericChanged;
        private bool isClicked = false;
        private Point previousPoint = new Point(0,0);
        public float ResetValue = 0;
        public float? MinValue = null;

        public NumericSpinner()
        {
            InitializeComponent();            
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CustomWindow w = (CustomWindow)Window.GetWindow(this);
            if (w != null)
            {
                w.MouseMoving += new MouseEventHandler(w_MouseMoving);
                w.MouseRightBtnUp += new MouseButtonEventHandler(w_MouseRightBtnUp);
            }  

            /*try
            {
                MainEditorWindow w = (MainEditorWindow)Window.GetWindow(this);
                if (w != null)
                {
                    w.MouseMoving += new MouseEventHandler(w_MouseMoving);
                    w.MouseRightBtnUp += new MouseButtonEventHandler(w_MouseRightBtnUp);
                }
            }
            catch
            {
                MainEditorWindow w = (MainEditorWindow)Window.GetWindow(this).Owner;
                if (w != null)
                {
                    w.MouseMoving += new MouseEventHandler(w_MouseMoving);
                    w.MouseRightBtnUp += new MouseButtonEventHandler(w_MouseRightBtnUp);
                }
            } */           
        }

        

        void w_MouseMoving(object sender, MouseEventArgs e)
        {
            if (isClicked)
            {
                Mouse.SetCursor(Cursors.ScrollNS);
                float adjVal = 0.05f;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                    adjVal = 1;
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    adjVal = 3;
                

                float n = NumericValue;
                Point p = Mouse.GetPosition(btnScrubber);
                n += (float)((previousPoint.Y - p.Y) * adjVal);
                if (MinValue.HasValue && n < MinValue.Value) n = MinValue.Value;

                NumericValue = n;
                if (NumericChanged != null) NumericChanged(sender, e);
                previousPoint = p;
            }
        }
        


        public float NumericValue
        {
            get
            {
                if (string.IsNullOrEmpty(txtValue.Text))
                    return 0;
                
                return (float)Math.Round(double.Parse(txtValue.Text), 2);
            }
            set
            {
                txtValue.Text = value.ToString();
            }
        }

        void w_MouseRightBtnUp(object sender, MouseButtonEventArgs e)
        {
            isClicked = false;
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
        private void btnScrubber_Click(object sender, RoutedEventArgs e)
        {
            NumericValue = ResetValue;
            if (NumericChanged != null) NumericChanged(sender, e);
        }                
    }
}
