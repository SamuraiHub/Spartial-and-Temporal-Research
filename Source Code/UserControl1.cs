using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Spatial_and_Temporal_Research
{
    public partial class UserControl1 : DockContent
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        public void load(EventArgs e)
        {
            base.OnLoad(e);
        }
    }
}
