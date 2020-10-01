using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Autodesk.Navisworks.Api;

namespace Spatial_and_Temporal_Research
{
    public partial class DataProperties : DockContent
    {
        public DataProperties()
        {
            InitializeComponent();

            this.tabControl = new System.Windows.Forms.TabControl();
            this.Property = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl.SuspendLayout();
            this.Property.Width = 123;
            this.Value.Width = 214;

            // tabControl
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(200, 565);
            this.tabControl.TabIndex = 0;
            this.tabControl.ResumeLayout(false);
     
            // Property
            this.Property.Text = "Property";

            // Value 
            this.Value.Text = "Value";   
        }

        private System.Windows.Forms.TabPage TabPage(String txt, int p)
        {
            System.Windows.Forms.TabPage tabPage = new TabPage(txt);
            tabPage.SuspendLayout();

            //tabPage.Controls.Add(this.listView);
            tabPage.Location = new System.Drawing.Point(4, 4);
            tabPage.Name = "tabPage"+p;
            tabPage.Padding = new System.Windows.Forms.Padding(3);
            tabPage.Size = new System.Drawing.Size(200, 539);
            tabPage.TabIndex = p;
            tabPage.UseVisualStyleBackColor = true;
            tabPage.AutoScroll = true;
            tabPage.ResumeLayout(false);

            return tabPage;
        }

        private System.Windows.Forms.ListView ListView(int l)
        {
            System.Windows.Forms.ListView listView = new System.Windows.Forms.ListView();

            //listView.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            listView.AutoArrange = false;
            listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            (ColumnHeader) this.Property.Clone(),
            (ColumnHeader) this.Value.Clone()});
            listView.Dock = System.Windows.Forms.DockStyle.Fill;
            listView.Location = new System.Drawing.Point(3, 3);
            listView.MultiSelect = false;
            listView.Name = "listView"+l;
            listView.Size = new System.Drawing.Size(194, 533);
            listView.TabIndex = 0;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = System.Windows.Forms.View.Details;
            listView.VirtualListSize = 5;
            return listView;
        }

        // displays the properties of the model item

        public void displayProperties(PropertyCategoryCollection PCC)
        {
            this.PCC = PCC;
            tabControl.Controls.Clear();

            if (!t)
            {
                t = true;
                this.Controls.RemoveAt(0);
                this.Controls.Add(tabControl);
            }

            int p = 0;

            foreach (PropertyCategory PC in PCC)
            {  
                tabControl.Controls.Add(TabPage(PC.DisplayName, p)); 
                
                System.Windows.Forms.ListView listView = ListView(p);

                tabControl.Controls[p].Controls.Add(listView); p++;

                if (PC.DisplayName == "Dynamics")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        DataProperty DP = PC.Properties[i];

                        ListViewItem lvi = new ListViewItem(new[] { DP.DisplayName, DP.Value.ToString().Substring(DP.Value.ToString().IndexOf(':') + 1) });

                        listView.Items.Add(lvi);
                    }
                }

                else
                {
                    foreach (DataProperty DP in PC.Properties)
                    {
                        ListViewItem lvi = new ListViewItem(new[] { DP.DisplayName, DP.Value.ToString().Substring(DP.Value.ToString().IndexOf(':') + 1) });

                        listView.Items.Add(lvi);
                    }
                }
            }
        }

        public void displayLabel(String txt)
        {
            if (t)
            {
                t = false;
                this.Controls.RemoveAt(0);
                this.Controls.Add(label);
            }

            label.Text = txt;
        }

        public bool T
        {
            get
            {
                return t;
            }
        }

        public PropertyCategoryCollection pcc
        {
            get
            {
                return PCC;
            }
        }
    }
}
