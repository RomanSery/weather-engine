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
    public partial class ObjectScale : UserControl
    {
        public event EventHandler ScaleChanged;
        public ObjectScale()
        {
            InitializeComponent();
            valX.ResetValue = valY.ResetValue = valZ.ResetValue = 1;
            valX.MinValue = valY.MinValue = valZ.MinValue = 0;
            valX.NumericChanged += new EventHandler(valX_NumericChanged);
            valY.NumericChanged += new EventHandler(valY_NumericChanged);
            valZ.NumericChanged += new EventHandler(valZ_NumericChanged);
        }

        void valX_NumericChanged(object sender, EventArgs e)
        {
            if (chkUniform.IsChecked.HasValue && chkUniform.IsChecked.Value) 
                valY.NumericValue = valZ.NumericValue = valX.NumericValue;            

            if (ScaleChanged != null) ScaleChanged(sender, e);
        }
        void valY_NumericChanged(object sender, EventArgs e)
        {
            if (chkUniform.IsChecked.HasValue && chkUniform.IsChecked.Value)
                valX.NumericValue = valZ.NumericValue = valY.NumericValue;            

            if (ScaleChanged != null) ScaleChanged(sender, e);
        }
        void valZ_NumericChanged(object sender, EventArgs e)
        {
            if (chkUniform.IsChecked.HasValue && chkUniform.IsChecked.Value)
                valX.NumericValue = valY.NumericValue = valZ.NumericValue;

            if (ScaleChanged != null) ScaleChanged(sender, e);
        }

        public Vector3 Scale
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
    }
}
