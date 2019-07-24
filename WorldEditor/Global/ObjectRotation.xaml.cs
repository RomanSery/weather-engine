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

using SlimDX;

namespace WorldEditor.Controls
{
    /// <summary>
    /// Interaction logic for Position.xaml
    /// </summary>
    public partial class ObjectRotation : UserControl
    {
        public event EventHandler RotationChanged;
        public ObjectRotation()
        {
            InitializeComponent();
            valX.NumericChanged += new EventHandler(val_NumericChanged);
            valY.NumericChanged += new EventHandler(val_NumericChanged);
            valZ.NumericChanged += new EventHandler(val_NumericChanged);
        }

        void val_NumericChanged(object sender, EventArgs e)
        {
            if (RotationChanged != null)
                RotationChanged(sender, e);
        }

        public SlimDX.Matrix Rotation
        {
            get
            {
                SlimDX.Matrix matRotation;
                matRotation = SlimDX.Matrix.RotationX(ToRadians(valX.NumericValue)) * SlimDX.Matrix.RotationY(ToRadians(valY.NumericValue)) * SlimDX.Matrix.RotationZ(ToRadians(valZ.NumericValue));
                return matRotation;                
            }           
        }
        public Vector3 RotationValues
        {
            get
            {
                return new Vector3(valX.NumericValue, valY.NumericValue, valZ.NumericValue);                
            }
            set
            {
                valX.NumericValue = value.X;
                valY.NumericValue = value.Y;
                valZ.NumericValue = value.Z;
            }
        }

        private float ToRadians(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }
    }
}
