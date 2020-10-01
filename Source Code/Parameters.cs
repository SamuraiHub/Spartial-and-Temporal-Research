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
    public partial class Parameters : Form
    {
        public Parameters(int[] durations)
        {
            InitializeComponent();
            this.durations = durations; ds = new int[durations.Length];
            population.Text = "2"; p = 2; pt = true;
            iteration.Text = "1"; t = 1; tt = true; dt = true;
            for (int i = 0; i < durations.Length; i++) { ds[i] = 2; }
        }

        private void population_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                p = int.Parse(population.Text);
                errorProvider1.SetError(population, "");
                pt = true;

                if (p < 2 || p > 100)
                {
                    errorProvider1.SetError(population, "Must be in the range 2 to 100");
                    pt = false;
                    Run.DialogResult = System.Windows.Forms.DialogResult.None;
                }

                if (pt && tt & dt)
                    Run.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider1.SetError(population, "Not an integer value.");
                pt = false;
                Run.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void iteration_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                t = int.Parse(iteration.Text);
                errorProvider1.SetError(iteration, "");
                tt = true;

                if (p < 2 || p > 100)
                {
                    errorProvider1.SetError(iteration, "Must be in the range 1 to 100");
                    tt = false;
                    Run.DialogResult = System.Windows.Forms.DialogResult.None;
                }

                if (pt && tt & dt)
                    Run.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider1.SetError(iteration, "Not an integer value.");
                tt = false;
                Run.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        public int getSelectedSetIndex
        {
            get
            {
                return comboBox1.SelectedIndex;
            }
        }

        public int getSelectedMethodIndex
        {
            get
            {
                return comboBox2.SelectedIndex;
            }
        }

        public void setSetItems(String[] items)
        {
                comboBox1.Items.AddRange(items);
                comboBox1.SelectedIndex = 0;
                comboBox3.Items.AddRange(items);
                comboBox3.Items.RemoveAt(0);
                comboBox3.SelectedIndex = -1;
        }

        public void setSelecMethods(String[] items)
        {
            comboBox2.Items.AddRange(items);
            comboBox2.SelectedIndex = 0;
        }

        public int getPop
        {
            get
            {
              return p;
            }
        }

        public int getItr
        {
            get
            {
             return t;
            }
        }

        private void Run_Click(object sender, EventArgs e)
        {
            if (!pt)
            {
                MessageBox.Show(this, "Population number must be a valid integer value in the range 2 to 100", "Integer parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else if(!tt)
            {
                MessageBox.Show(this, "Number of iterations must be a valid integer value in the range 1 to 100", "Integer parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else if (!dt)
            {
                MessageBox.Show(this, "Duration number must be a valid integer value in the range 2 to "+durations[comboBox3.SelectedIndex], "Integer parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Enabled = true; textBox1.Text = "" + ds[comboBox3.SelectedIndex];
        }

        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                ds[comboBox3.SelectedIndex] = int.Parse(textBox1.Text);
                errorProvider1.SetError(textBox1, "");
                dt = true; comboBox3.Enabled = true;

                if (ds[comboBox3.SelectedIndex] < 2 || ds[comboBox3.SelectedIndex] > durations[comboBox3.SelectedIndex])
                {
                    errorProvider1.SetError(textBox1, "Must be in the range 2 to " + durations[comboBox3.SelectedIndex]);
                    dt = false; comboBox3.Enabled = false;
                    Run.DialogResult = System.Windows.Forms.DialogResult.None;
                }

                if (pt && tt & dt)
                    Run.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                errorProvider1.SetError(textBox1, "Not an integer value.");
                dt = false; comboBox3.Enabled = false;
                Run.DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        public int[] Durations
        {
            get
            {
                return ds;
            }
        }
    }
}
