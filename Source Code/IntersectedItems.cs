using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Spatial_and_Temporal_Research
{
    public partial class IntersectedItems : Form
    {
        public IntersectedItems()
        {
            InitializeComponent();
        }

        public TextBox TextBox
        {
            get
            {
                return textBox1;
            }
        }
    }
}
