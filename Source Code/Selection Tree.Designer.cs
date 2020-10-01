namespace Spatial_and_Temporal_Research
{
    partial class Selection_Tree
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Selection_Tree));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.msTreeView1 = new MSTreeView.MSTreeView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changePropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDesendantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(217, 597);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.msTreeView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(209, 571);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Items";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // msTreeView1
            // 
            this.msTreeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.msTreeView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.msTreeView1.Location = new System.Drawing.Point(3, 3);
            this.msTreeView1.Name = "msTreeView1";
            this.msTreeView1.SelectedNodes = ((System.Collections.Generic.List<MSTreeView.MSTreeNode>)(resources.GetObject("msTreeView1.SelectedNodes")));
            this.msTreeView1.Size = new System.Drawing.Size(203, 565);
            this.msTreeView1.TabIndex = 0;
            this.msTreeView1.SelectedNodesChanged += new System.EventHandler(this.msTreeView1_SelectedNodesChanged);
            this.msTreeView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.msTreeView1_MouseUp);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listView1);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(209, 571);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Sets";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.listView1.AutoArrange = false;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(203, 565);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.VirtualListSize = 10;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 200;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changePropertiesToolStripMenuItem, this.changeDesendantsToolStripMenuItem });
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(164, 70);
            // 
            // changePropertiesToolStripMenuItem
            // 
            this.changePropertiesToolStripMenuItem.Name = "changePropertiesToolStripMenuItem";
            this.changePropertiesToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.changePropertiesToolStripMenuItem.Text = "Change Properties";
            this.changePropertiesToolStripMenuItem.Click += new System.EventHandler(this.changePropertiesToolStripMenuItem_Click);
            // 
            // changeDesendantsToolStripMenuItem
            // 
            this.changeDesendantsToolStripMenuItem.Name = "changeDesendantsToolStripMenuItem";
            this.changeDesendantsToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.changeDesendantsToolStripMenuItem.Text = "Change Desendants Properties";
            this.changeDesendantsToolStripMenuItem.Click += new System.EventHandler(this.changeDesendantsToolStripMenuItem_Click);
            // 
            // Selection_Tree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(217, 597);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Selection_Tree";
            this.Text = "Selection Tree";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private Autodesk.Navisworks.Api.Document doc;
        private Autodesk.Navisworks.Api.ModelItemCollection modelSelection;
        private System.Windows.Forms.ListView listView1;
        private MSTreeView.MSTreeView msTreeView1;
        private DataProperties prop;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem changePropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeDesendantsToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private int i, j;
        private int[] ai, aj;
    }
}