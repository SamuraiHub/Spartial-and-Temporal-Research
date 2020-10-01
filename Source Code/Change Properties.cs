using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Spatial_and_Temporal_Research 
{
    public partial class Change_Properties : Form
    {
        public Change_Properties(double px, double mx, double py, double my, double pz, double mz, DateTime os, DateTime oe, bool m)
        {
            InitializeComponent(); this.os = os; this.oe = oe;
            bpx.Text = "" + px; bmx.Text = mx + ""; bpy.Text = py + ""; bmy.Text = my + ""; bpz.Text = pz + ""; bmz.Text = mz + "";
            this.Px = px; this.Mx = mx; this.Py = py; this.My = my; this.Pz = pz; this.Mz = mz;
            vpx = true; vmx = true; vpy = true; vmy = true; vpz = true; vmz = true; DCB.SelectedIndex = 0;
            this.ActiveControl = this.px;
            this.ActiveControl.TabIndex = 0;

            SDTP.MinDate = os; SDTP.MaxDate = oe;
            EDTP.MinDate = os; EDTP.MaxDate = oe; 

            if (m)
            {
                SDCB.Hide(); SDTP.Location = SDCB.Location;
                EDCB.Hide(); EDTP.Location = EDCB.Location;
            }

            else
            {
                SDCB.SelectedIndex = 0; EDCB.SelectedIndex = 0;
                EDTP.Value = oe; EDTP.Enabled = false;
                SDTP.Value = os; SDTP.Enabled = false;
            }
        }
            
        private void button1_Click(object sender, EventArgs e)
        {
            if (!vpx || !vmx || !vpy || !vmy || !vpz || !vmz)
            {
                MessageBox.Show(this,"One of the protrusion fields has an invalid numerical value", "Double parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (EDTP.Value < SDTP.Value)
            {
                MessageBox.Show(this, "End Date cannot be before Start Date", "Out of order error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                d = ObjectItem.getDirection(DCB.Text); startDate = SDTP.Value; endDate = EDTP.Value;
            }
        }

        private void bmx_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Mx = Double.Parse(bmx.Text);
                errorProvider.SetError(bmx, "");
                vmx = true;

                if (vpx && vpy && vmy && vpz && vmz && EDTP.Value >= SDTP.Value)
                    OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider.SetError(bmx, "Not a numerical value.");
                vmx = false;
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void bpx_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Px = Double.Parse(bpx.Text);
                errorProvider.SetError(bpx, "");
                vpx = true;

                if (vmx && vpy && vmy && vpz && vmz && EDTP.Value >= SDTP.Value)
                    OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider.SetError(bpx, "Not a numerical value.");
                vpx = false;
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
            }

        }

        private void bpy_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Py = Double.Parse(bpy.Text);
                errorProvider.SetError(bpy, "");
                vpy = true;

                if (vpx && vmx && vmy && vpz && vmz && EDTP.Value >= SDTP.Value)
                    OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider.SetError(bpy, "Not a numerical value.");
                vpy = false;
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
            }

        }

        private void bpz_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Pz = Double.Parse(bpz.Text);
                errorProvider.SetError(bpz, "");
                vpz = true;

                if (vpx && vmx && vpy && vmy && vmz && EDTP.Value >= SDTP.Value)
                    OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider.SetError(bpz, "Not a numerical value.");
                vpz = false;
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
            }

        }

        private void bmy_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                My = Double.Parse(bmy.Text);
                errorProvider.SetError(bmy, "");
                vmy = true;

                if (vpx && vmx && vpy && vpz && vmz && EDTP.Value >= SDTP.Value)
                    OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider.SetError(bmy, "Not a numerical value.");
                vmy = false;
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
            }

        }

        private void bmz_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Mz = Double.Parse(bmz.Text);
                errorProvider.SetError(bmz, "");
                vmz = true;

                if (vpx && vmx && vpy && vmy && vpz && EDTP.Value >= SDTP.Value)
                    OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider.SetError(bmz, "Not a numerical value.");
                vmz = false;
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
            }

        }

        public double PX
        {
            get
            {
                return Px;
            }
        }

        public double MX
        {
            get
            {
                return Mx;
            }
        }

        public double PY
        {
            get
            {
                return Py;
            }
        }

        public double MY
        {
            get
            {
                return My;
            }
        }

        public double PZ
        {
            get
            {
                return Pz;
            }
        }

        public double MZ
        {
            get
            {
                return Mz;
            }
        }

        internal ObjectItem.direction D
        {
            get
            {
                return d;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
        }

        private void STCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SDCB.Text == "Specify") SDTP.Enabled = true;
            else { SDTP.Value = os; SDTP.Enabled = false; }
        }

        private void EDCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EDCB.Text == "Specify") EDTP.Enabled = true;
            else { EDTP.Value = oe; EDTP.Enabled = false; }
        }

        private void SDTP_ValueChanged(object sender, EventArgs e)
        {
            if (EDTP.Value < SDTP.Value)
            {
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (vpx && vmx && vpy && vmy && vpz && vmz && EDTP.Value >= SDTP.Value)
                OK.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void EDTP_ValueChanged(object sender, EventArgs e)
        {
            if (EDTP.Value < SDTP.Value)
            {
                OK.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (vpx && vmx && vpy && vmy && vpz && vmz && EDTP.Value >= SDTP.Value)
                OK.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
            
    }
}
