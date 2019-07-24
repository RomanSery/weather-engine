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
    public partial class Position : UserControl
    {
        public event EventHandler PositionChanged;
        public Position()
        {
            InitializeComponent();
            valX.NumericChanged += new EventHandler(val_NumericChanged);
            valY.NumericChanged += new EventHandler(val_NumericChanged);
            valZ.NumericChanged += new EventHandler(val_NumericChanged);
        }

        void val_NumericChanged(object sender, EventArgs e)
        {
            if (PositionChanged != null)
                PositionChanged(sender, e);
        }

        public Vector3 Pos
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
