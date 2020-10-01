using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Autodesk.Navisworks.Api.Controls;
using Autodesk.Navisworks.Api;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Spatial_and_Temporal_Research.Properties;

namespace Spatial_and_Temporal_Research
{
    public partial class Selection_Tree : DockContent
    {
        public Selection_Tree(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
            modelSelection = new ModelItemCollection();
            msTreeView1.ImageList = new ImageList();
            msTreeView1.ImageList.Images.Add(Resources.multi_sheet_file_type, System.Drawing.Color.Blue);
            msTreeView1.ImageList.Images.Add(Resources.Ribbon_GridsDisplayAll_16);
            msTreeView1.ImageList.Images.Add(Resources.Ribbon_Surfaces_16);
            msTreeView1.ImageList.Images.Add(Resources.SavedItem_Viewpoint_Orthographic);
            msTreeView1.ImageList.ImageSize = new Size(16, 16);

            listView1.SmallImageList = new ImageList();
            listView1.SmallImageList.Images.Add(Resources.SavedItem_Selection_Set);
            listView1.SmallImageList.ImageSize = new Size(16, 16);
        }

        // sets the items of the document to the selection tree for viewing

        public void setItems()
        {
            int i = 0;
            foreach (ModelItem m in doc.Models.RootItems)
            {
                MSTreeView.MSTreeNode r = new MSTreeView.MSTreeNode(); r.Text = m.DisplayName;
                msTreeView1.Nodes.Add(r);
                r.ImageIndex = 0;
                r.SelectedImageIndex = 0;

                itemLoop(m.Children, msTreeView1.Nodes[i], 1); i++;
            }
        }

        public void setItemSets()
        {
            for (int i = 0; i < doc.SelectionSets.Value.Count; i++)
            {
                listView1.Items.Add(doc.SelectionSets.Value[i].DisplayName, 0);
            }
        }


        /* loops on all items in the model collection tree (from the root to the leaves) and adds the 
         * their string names to the tree view. */

        private void itemLoop(ModelItemEnumerableCollection models, TreeNode tnc, int ii)
        {
            int j = 0;

            foreach (ModelItem m in models)
            {
                MSTreeView.MSTreeNode r = new MSTreeView.MSTreeNode();
                r.Text = m.PropertyCategories.ElementAt(0).Properties[0].Value.ToDisplayString();
                tnc.Nodes.Add(r);

                if (ii == 3 && m.Children.First != null)
                {
                    r.ImageIndex = ii - 1;
                    r.SelectedImageIndex = ii - 1;
                    itemLoop(m.Children, tnc.Nodes[j], ii);
                }

                else if (m.HasGeometry)
                {
                    r.ImageIndex = 3;
                    r.SelectedImageIndex = 3;
                }

                else
                {
                    r.ImageIndex = ii;
                    r.SelectedImageIndex = ii;
                    itemLoop(m.Children, tnc.Nodes[j], ii + 1);
                }
                
                j++;
            }
        }

        private void msTreeView1_SelectedNodesChanged(object sender, EventArgs e)
        {
            listView1.HideSelection = true;
            List<MSTreeView.MSTreeNode> sItems = msTreeView1.SelectedNodes;

            ((mainForm)this.DockPanel.Parent).timeSlider1_Scroll(sender, e);
            modelSelection.Clear(); 

            for (int j = 0; j < sItems.Count; j++)
            {
                int[] indexs = new int[10]; int i = 1;

                indexs[0] = sItems[j].Index; System.Windows.Forms.TreeNode n = sItems[j];

                while ((n = n.Parent) != null)
                {
                    indexs[i] = n.Index; i++;
                }

                ModelItem model = doc.Models.RootItems.ElementAt(indexs[i - 1]);

                for (i = i - 2; i >= 0; i--)
                {
                    model = model.Children.ElementAt(indexs[i]);
                }

                modelSelection.Add(model);
            }

            if (modelSelection.Count > 0)
            {
                System.Drawing.Color c = System.Drawing.Color.Blue;

                doc.Models.OverridePermanentColor(modelSelection, new Autodesk.Navisworks.Api.Color(c.R / 255, c.G / 255, c.B / 255));
            }

            if (modelSelection.Count == 1) prop.displayProperties(modelSelection[0].PropertyCategories);
            else prop.displayLabel(modelSelection.Count + " items selected");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((mainForm)this.DockPanel.Parent).timeSlider1_Scroll(sender, e);
            //doc.Models.ResetPermanentMaterials(modelSelection);
            modelSelection.Clear(); listView1.HideSelection = false;
            
            SavedItemCollection si = doc.SelectionSets.Value;

            System.Windows.Forms.ListView.SelectedIndexCollection indicies = listView1.SelectedIndices;

            for (int i = 0; i < indicies.Count; i++)
            {
                SelectionSource ss = doc.SelectionSets.CreateSelectionSource(si[indicies[i]]);
                Search s = new Search(); s.Selection.SelectionSources.Add(ss);

                modelSelection.AddRange(s.Selection.GetSelectedItems(doc));
            }

            msTreeView1.CollapseAll();

            if (modelSelection.Count > 0)
            {
                System.Drawing.Color c = System.Drawing.Color.Blue;

                doc.Models.OverridePermanentColor(modelSelection, new Autodesk.Navisworks.Api.Color(c.R / 255, c.G / 255, c.B / 255));

                List<MSTreeView.MSTreeNode> selectedNodes = new List<MSTreeView.MSTreeNode>();

                int j = 0;
                foreach (ModelItem m in doc.Models.RootItems)
                {
                    if (modelSelection.IsSelected(m))
                    {
                        selectedNodes.Add((MSTreeView.MSTreeNode)msTreeView1.Nodes[j]);
                    }
                    else
                    {
                        if (treeSelect(m.Children, msTreeView1.Nodes[j], selectedNodes))
                        {
                            msTreeView1.Nodes[j].Expand();
                        }
                    }
                    j++;
                }

                msTreeView1.SelectedNodes = selectedNodes;
            }
            else
            {
                msTreeView1.SelectedNodes = new List<MSTreeView.MSTreeNode>();
            }

            if (modelSelection.Count == 1) prop.displayProperties(modelSelection[0].PropertyCategories);
            else prop.displayLabel(modelSelection.Count + " items selected");
        }

        private bool treeSelect(ModelItemEnumerableCollection models, TreeNode tnc, List<MSTreeView.MSTreeNode> selectedNodes)
        {
            if (models.First == null)
            {
                return false; 
            }
            int j = 0; bool s = false;

            foreach (ModelItem m in models)
            {
                if (modelSelection.IsSelected(m))
                {
                    s = true;
                    selectedNodes.Add((MSTreeView.MSTreeNode)tnc.Nodes[j]);
                }
                else
                {
                    if (treeSelect(m.Children, tnc.Nodes[j], selectedNodes))
                    {
                        s = true;
                        tnc.Nodes[j].Expand();
                    }
                }
                j++;
            }

            return s;
        }

        public DataProperties Prop
        {
            set
            {
                prop = value;
            }
        }

        private void msTreeView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                MSTreeView.MSTreeNode node = msTreeView1.GetNodeAt(e.Location) as MSTreeView.MSTreeNode;

                if (node != null)
                {
                    if (e.Location.X >= node.Bounds.X - (node.TreeView.ImageList.ImageSize.Width + 5) &&
                        e.Location.X <= node.Bounds.Right + 1)
                    {
                        ((mainForm)this.DockPanel.Parent).timeSlider1_Scroll(sender, e);

                        System.Drawing.Color c = System.Drawing.Color.Blue;

                        int[] indexs = new int[10]; int i = 1;

                        indexs[0] = node.Index; System.Windows.Forms.TreeNode n = node;

                        while ((n = n.Parent) != null)
                        {
                            indexs[i] = n.Index; i++;
                        }

                        ModelItem model = doc.Models.RootItems.ElementAt(indexs[i - 1]);

                        for (i = i - 2; i >= 0; i--)
                        {
                            model = model.Children.ElementAt(indexs[i]);
                        }

                        doc.Models.OverridePermanentColor(model.Self, new Autodesk.Navisworks.Api.Color(c.R / 255, c.G / 255, c.B / 255));

                        if (model.PropertyCategories.ElementAt(model.PropertyCategories.Count() - 1).DisplayName == "Dynamics")
                        {
                            this.i = model.PropertyCategories.ElementAt(model.PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                            j = model.PropertyCategories.ElementAt(model.PropertyCategories.Count() - 1).Properties[5].Value.ToInt32();
                       
                                contextMenuStrip1.Items.Clear();

                                contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                                changePropertiesToolStripMenuItem });

                                contextMenuStrip1.Size = new Size(164, 26);
                                contextMenuStrip1.Show(msTreeView1, e.Location);
                        }
                        else
                        {
                            ModelItemCollection modelC = new ModelItemCollection(); 
                            ModelItemEnumerableCollection modelItems = model.Descendants;

                            foreach (ModelItem m in modelItems)
                            {
                                if (m.PropertyCategories.ElementAt(m.PropertyCategories.Count() - 1).DisplayName ==
                                            "Dynamics" && m.Parent.PropertyCategories.ElementAt(m.Parent.PropertyCategories.Count() - 1).DisplayName !=
                            "Dynamics")
                                {
                                    modelC.Add(m);
                                }
                            }

                            if (modelC.Count > 0)
                            {
                                ai = new int[modelC.Count]; aj = new int[modelC.Count];

                                for (i = 0; i < modelC.Count; i++)
                                {
                                    model = modelC[i];

                                    ai[i] = model.PropertyCategories.ElementAt(model.PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                                    aj[i] = model.PropertyCategories.ElementAt(model.PropertyCategories.Count() - 1).Properties[5].Value.ToInt32();
                                }
                                contextMenuStrip1.Items.Clear();
                                contextMenuStrip1.Items.Add(changeDesendantsToolStripMenuItem);
                                contextMenuStrip1.Size = new Size(164, 26);
                                contextMenuStrip1.Show(msTreeView1, e.Location);
                            }
                        }
                    }
                }
            }
        }

        private void changePropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {  
                ((mainForm)this.DockPanel.Parent).change_itemProperties(i, j); 
        }

        private void changeDesendantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
                ((mainForm)this.DockPanel.Parent).change_itemProperties(ai, aj);
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem item = listView1.GetItemAt(e.X,e.Y);

                if (item != null)
                {
                    if (e.Location.X >= item.Bounds.X - (item.ListView.SmallImageList.ImageSize.Width + 5) &&
                        e.Location.X <= item.Bounds.Right + 1)
                    {
                        ((mainForm)this.DockPanel.Parent).timeSlider1_Scroll(sender, e);

                        SavedItemCollection si = doc.SelectionSets.Value;

                        SelectionSource ss = doc.SelectionSets.CreateSelectionSource(si[item.Index]);
                        Search s = new Search(); s.Selection.SelectionSources.Add(ss);

                        modelSelection = new ModelItemCollection(s.Selection.GetSelectedItems(doc));

                        System.Drawing.Color c = System.Drawing.Color.Blue;

                        doc.Models.OverridePermanentColor(modelSelection, new Autodesk.Navisworks.Api.Color(c.R / 255, c.G / 255, c.B / 255));

                        i = modelSelection[0].PropertyCategories.ElementAt(modelSelection[0].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32(); 
                        j = -1;
                        contextMenuStrip1.Items.Clear();
                        contextMenuStrip1.Items.Add(changePropertiesToolStripMenuItem);
                        contextMenuStrip1.Size = new Size(164, 26);
                        contextMenuStrip1.Show(msTreeView1, e.Location);
                    }
                }
            }
        }

        public ModelItemCollection ModelSelection
        {
            get
            {
                return modelSelection;
            }
        }
    }
}
