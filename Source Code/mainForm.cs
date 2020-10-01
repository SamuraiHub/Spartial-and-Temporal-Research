using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Navisworks.Api.Controls;
using Autodesk.Navisworks.Api;
using System.Diagnostics;
using System.Collections;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms.DataVisualization.Charting;
using WeifenLuo.WinFormsUI.Docking;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api.ComApi;
using OpenTK;

namespace Spatial_and_Temporal_Research
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent(); currentDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Selection_Tree = new Selection_Tree(documentControl.Document); 
            Properties = new DataProperties();
            Selection_Tree.Prop = Properties;
            displayControl = new UserControl1(); //displayControl.Size = viewControl.Size;
            displayControl.CloseButtonVisible = false;
            displayControl.Controls.Add(this.timeSlider1); displayControl.Controls.Add(this.button1);
            displayControl.Controls.Add(viewControl);
            displayControl.Show(dockPanel1, DockState.Document);
            Selection_Tree.HideOnClose= true;
            Selection_Tree.Show(dockPanel1, DockState.DockLeft);
            Properties.Show(dockPanel1, DockState.DockRight);
            Properties.HideOnClose = true;
            precalc = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            timeSlider1.Location = new Point(timeSlider1.Location.X, timeSlider1.Location.Y - 33);
            button1.Location = new Point(button1.Location.X, button1.Location.Y - 33);
            glControl1.MakeCurrent();
            GL.ClearColor(System.Drawing.Color.Black); // Yey! .NET Colors can be used directly!
            //SetupViewport();
        }

        private DateTime[] Clone(DateTime[] dts)
        {
            DateTime[] a = new DateTime[dts.Length];
            Array.Copy(dts, a, dts.Length);
            return a;
        }

        private void LineChart_Load(object sender, EventArgs e)
        {

        }

        public void addData(double[] Data, DateTime[] Time)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                LineChart.Series[0].Points.AddXY(Time[i], Data[i]);
            }

            DataPoint maxDataPoint = LineChart.Series[0].Points.FindMaxByValue();
            LineChart.ChartAreas[0].AxisY.Interval = maxDataPoint.YValues[0] / 20;            
        }

        private void setVisible()
        {
            int y = 0;
            ModelItemCollection modelItems = new ModelItemCollection();

            while (y < SS.Length && SS[sT[y]] <= timeSlider1.Value)
            {
                if (SE[sT[y]] > timeSlider1.Value)
                {
                    documentControl.Document.Models.OverridePermanentColor(SI[y].Self, Autodesk.Navisworks.Api.Color.FromByteRGB(255, 255, 0));
                }

                modelItems.Add(SI[sT[y]]);

                y++;
            }

            if (modelItems.Count != 0)
            {
                documentControl.Document.Models.SetHidden(modelItems, false);
            }

            if (y == SS.Length && SE[edp[y - 1]] <= timeSlider1.Value)
            {
                documentControl.Document.Models.SetHidden(SItems, false);
            }
        }

        private void setBoundingBoxesVisible()
        {
            int y = 0;

            while (y < SS.Length && SS[sT[y]] <= timeSlider1.Value)
            {
                System.Drawing.Color color;

                if (SE[sT[y]] > timeSlider1.Value)
                {
                    color = System.Drawing.Color.Yellow;
                }
                else
                {
                    color = System.Drawing.Color.Gray;
                }

                Autodesk.Navisworks.Api.Point3D bn1 = SI[sT[y]].BoundingBox().Min;
                Autodesk.Navisworks.Api.Point3D bx1 = SI[sT[y]].BoundingBox().Max;

                BoundingBox3D bb1 = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(bn1.X, bn1.Y, bn1.Z),
                                                  new Autodesk.Navisworks.Api.Point3D(bx1.X, bx1.Y, bx1.Z));

                        Draw3DRec(bb1.Min, bb1.Max, color);

                y++;
            }

            if (y == SS.Length && SE[edp[y - 1]] <= timeSlider1.Value)
            {
                for (int j = 0; j < SItems.Count; j++)
                {
                    Autodesk.Navisworks.Api.Point3D bn1 = SItems[j].BoundingBox().Min;
                    Autodesk.Navisworks.Api.Point3D bx1 = SItems[j].BoundingBox().Max;

                    BoundingBox3D bb1 = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(bn1.X, bn1.Y, bn1.Z),
                                                          new Autodesk.Navisworks.Api.Point3D(bx1.X, bx1.Y, bx1.Z));

                    Draw3DRec(bb1.Min, bb1.Max, System.Drawing.Color.Gray);
                }
            }
        }

        private void preCalculate()
        {
            mics = new ModelItemCollection[SI.Length * 2]; ModelItemCollection mItems = new ModelItemCollection();

            ADates = new DateTime[SI.Length * 2]; Array.Copy(SS, ADates, SI.Length);

            Array.Copy(SE, 0, ADates, SI.Length, SI.Length);

            bool[] ISED = new bool[SI.Length * 2]; int k; for (k = 0; k < SI.Length; k++) { ISED[k] = false; } ISMIE = new bool[SI.Length * 2][];

            for (k = SI.Length; k < SI.Length * 2; k++) { ISED[k] = true; } Array.Sort(ADates, ISED); 

            k = 0;
            for (int i = 0, j = 0; i < ADates.Length; i++)
            {
                if (ISED[i])
                {
                    ISMIE[i] = new bool[j];
                    Array.Copy(ISMIE[i - 1], ISMIE[i], ISMIE[i - 1].Length);
                    ISMIE[i][edp[k]] = true;
                    k++;
                }
                else
                {
                    mItems.Add(SI[sT[j]]);
                    j++;
                    ISMIE[i] = new bool[j];
                    if(i > 0)
                    Array.Copy(ISMIE[i - 1], ISMIE[i], ISMIE[i - 1].Length);
                }

                mics[i] = new ModelItemCollection(mItems);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
             //            #region DocumentControlDocumentTryOpenFile
            //Dialog for selecting the Location of the file toolStripMenuItem1 open
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = "nwd";
            dlg.Filter = "Navisworks Files (*.nwc;*.nwd;*.nwf)|*.nwc;*.nwd;*.nwf";

            //Ask user for file location
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadDocument(dlg.FileName, dlg.FileName.Substring(0, dlg.FileName.Length - 3) + "csv");
            }
        }

        private double getIntersectsVolume(int si, DateTime date, ref double[] intfm, ref double[] intsm)
        {
            int y = setI[si], i = 0, j = 0, m = setI[si + 1] - setI[si]; int[] i1 = new int[m]; int[] i2 = new int[m];

            ModelItemCollection mc1 = new ModelItemCollection();
            ModelItemCollection mc2 = new ModelItemCollection();

            while (y < setI[si + 1] && SS[sT[y]] <= date)
            {
                if (SE[sT[y]] > date)
                {
                    mc2.Add(SI[sT[y]]);
                    i2[j] = sT[y]; j++;
                }

                else
                {
                    mc1.Add(SI[sT[y]]);
                    i1[i] = sT[y]; i++;
                }

                y++;
            }

            Array.Resize(ref i2, j); Array.Resize(ref i1, i);

            double v = 0;

            if (si == 0)
            {
                m = Items.Length; intfm = new double[m * m]; intsm = new double[m];
            }

            else
            {
               intfm = new double[m * m]; intsm = new double[SItems.Count * m];
            }

            if (mc2.Count > 0)
            {
                v = v + getBiSectVolume(si, date, SItems, mc2, i1, i2, intfm, intsm, false);

                if (mc1.Count > 0)
                {
                    v = v + getBiSectVolume(si, date, mc1, mc2, i1, i2, intfm, intsm, true);
                }

              for (int k = 0; k < mc2.Count; k++)
                {
                    Autodesk.Navisworks.Api.Point3D bn1 = mc2[k].BoundingBox().Min;
                    Autodesk.Navisworks.Api.Point3D bx1 = mc2[k].BoundingBox().Max;

                    Autodesk.Navisworks.Api.Point3D Max =
                        new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048));

                    Autodesk.Navisworks.Api.Point3D Min =
                        new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048));

                    BoundingBox3D bb1 = new BoundingBox3D(Min, Max);

                        double mw = (date - SS[i2[k]]).Days * Micp[i2[k]].Rate;

                        if (Micp[i2[k]].Direction == ObjectItem.direction.Right)
                        {
                            Min = new Autodesk.Navisworks.Api.Point3D(Min.X + mw, Min.Y, Min.Z).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());
                            Max = new Autodesk.Navisworks.Api.Point3D(Min.X + mw + Micp[i2[k]].Rate, Max.Y, Max.Z).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());
                        }

                        else if (Micp[i2[k]].Direction == ObjectItem.direction.Left)
                        {
                            Min = new Autodesk.Navisworks.Api.Point3D(Max.X - (mw + Micp[i2[k]].Rate), Min.Y, Min.Z).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());
                            Max = new Autodesk.Navisworks.Api.Point3D(Max.X - mw, Max.Y, Max.Z).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());
                        }

                        else if (Micp[i2[k]].Direction == ObjectItem.direction.Up)
                        {
                            Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y + mw, Min.Z).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());
                            Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Min.Y + mw + Micp[i2[k]].Rate, Max.Z).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());
                        }

                        else if (Micp[i2[k]].Direction == ObjectItem.direction.Down)
                        {
                            Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Max.Y - (mw + Micp[i2[k]].Rate), Min.Z).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());
                            Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y - mw, Max.Z).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());
                        }

                        else if (Micp[i2[k]].Direction == ObjectItem.direction.Front)
                        {
                            Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y, Min.Z + mw).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());
                            Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y, Min.Z + mw + Micp[i2[k]].Rate).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());
                        }

                        else
                        {
                            Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y, Max.Z - (mw + Micp[i2[k]].Rate)).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());
                            Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y, Max.Z - mw).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());
                        }

                        BoundingBox3D BBP = new BoundingBox3D(Min, Max);

                    for (i = k + 1; i < mc2.Count; i++)
                    {
                        bn1 = mc2[i].BoundingBox().Min;
                        bx1 = mc2[i].BoundingBox().Max;

                        Max =
                            new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048));

                        Min =
                            new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048));

                        BoundingBox3D bb2 = new BoundingBox3D(Min, Max);
                            
                        mw = (date - SS[i2[i]]).Days * Micp[i2[i]].Rate;

                            if (Micp[i2[i]].Direction == ObjectItem.direction.Right)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X + mw, Min.Y, Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Min.X + mw + Micp[i2[i]].Rate, Max.Y, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Left)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Max.X - (mw + Micp[i2[i]].Rate), Min.Y, Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X - mw, Max.Y, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Up)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y + mw, Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Min.Y + mw + Micp[i2[i]].Rate, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Down)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Max.Y - (mw + Micp[i2[i]].Rate), Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y - mw, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Front)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y, Min.Z + mw).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y, Min.Z + mw + Micp[i2[i]].Rate).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y, Max.Z - (mw + Micp[i2[i]].Rate)).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y, Max.Z - mw).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            BoundingBox3D BBP1 = new BoundingBox3D(Min, Max);

                            if (BBP.Intersects(bb2) || BBP1.Intersects(bb1) || BBP.Intersects(BBP1))
                            {
                                double vol = BBP.Intersect(bb2).Volume + BBP1.Intersect(bb1).Volume + BBP.Intersect(BBP1).Volume
                                    - BBP.Intersect(bb2).Intersect(BBP1).Volume - BBP1.Intersect(bb1).Intersect(BBP).Volume;
                                v = v + vol;

                                if (si == 0)
                                {
                                    int ik = mc2[k].PropertyCategories.ElementAt(mc2[k].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                                    int ii = mc2[i].PropertyCategories.ElementAt(mc2[i].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                                    intfm[(ik * m) + ii] += vol; intfm[(ii * m) + ik] += vol;
                                }

                                else
                                {
                                    int a = setI[si] - setI[1];
                                    intfm[((i2[k] - a) * m) + (i2[i] - a)] += vol; intfm[((i2[i] - a) * m) + (i2[k] - a)] += vol;
                                }
                            }
                    }
                } 
            } 

            return v;
        }

        private double getBiSectVolume(int si, DateTime date, ModelItemCollection mc1, ModelItemCollection mc2, int[] i1, int[] i2, double[] intfm, double[] intsm, bool isD)
        {
            double v = 0;

            double mx = mc1[0].BoundingBox().Min.X * 0.3048; double my = mc1[0].BoundingBox().Min.Y * 0.3048; double mz = mc1[0].BoundingBox().Min.Z * 0.3048;
            double px = mc1[0].BoundingBox().Max.X * 0.3048; double py = mc1[0].BoundingBox().Max.Y * 0.3048; double pz = mc1[0].BoundingBox().Max.Z * 0.3048;

            Autodesk.Navisworks.Api.Point3D bn1;
            Autodesk.Navisworks.Api.Point3D bx1;

            for (int k = 1; k < mc1.Count; k++)
            {
                bn1 = mc1[k].BoundingBox().Min;
                bx1 = mc1[k].BoundingBox().Max;

                Autodesk.Navisworks.Api.Point3D Max =
                    new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048));

                Autodesk.Navisworks.Api.Point3D Min =
                    new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048));

               /*if (isD)
                {
                    Max = Max.Add(Micp[i1[k]].Protrusion.Max.ToVector3D());
                    Min = Min.Subtract(Micp[i1[k]].Protrusion.Min.ToVector3D());
                }*/

                mx = Math.Min(mx, Min.X);
                my = Math.Min(my, Min.Y);
                mz = Math.Min(mz, Min.Z);
                px = Math.Max(px, Max.X);
                py = Math.Max(py, Max.Y);
                pz = Math.Max(pz, Max.Z); 
            }

            bn1 = new Autodesk.Navisworks.Api.Point3D(mx, my, mz);
            bx1 = new Autodesk.Navisworks.Api.Point3D(px, py, pz);

            BoundingBox3D bb1 = new BoundingBox3D(bn1, bx1);

            mx = mc2[0].BoundingBox().Min.X * 0.3048; my = mc2[0].BoundingBox().Min.Y * 0.3048; mz = mc2[0].BoundingBox().Min.Z * 0.3048;
            px = mc2[0].BoundingBox().Max.X * 0.3048; py = mc2[0].BoundingBox().Max.Y * 0.3048; pz = mc2[0].BoundingBox().Max.Z * 0.3048;

            for (int k = 0; k < mc2.Count; k++)
            {
                bn1 = mc2[k].BoundingBox().Min;
                bx1 = mc2[k].BoundingBox().Max;

                Autodesk.Navisworks.Api.Point3D Max =
                    new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048)).Add(Micp[i2[k]].Protrusion.Max.ToVector3D());

                Autodesk.Navisworks.Api.Point3D Min =
                    new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048)).Subtract(Micp[i2[k]].Protrusion.Min.ToVector3D());

                mx = Math.Min(mx, Min.X);
                my = Math.Min(my, Min.Y);
                mz = Math.Min(mz, Min.Z);
                px = Math.Max(px, Max.X);
                py = Math.Max(py, Max.Y);
                pz = Math.Max(pz, Max.Z);
            }

            bn1 = new Autodesk.Navisworks.Api.Point3D(mx, my, mz);
            bx1 = new Autodesk.Navisworks.Api.Point3D(px, py, pz);

            BoundingBox3D bb2 = new BoundingBox3D(bn1, bx1);

            if (bb1.Intersects(bb2))
            {

                BoundingBox3D intrsct = bb1.Intersect(bb2);

                List<BoundingBox3D> im = new List<BoundingBox3D>(mc1.Count);
                List<int> ml = new List<int>(mc1.Count);
                int i;
                for (i = 0; i < mc1.Count; i++)
                {
                    bn1 = mc1[i].BoundingBox().Min;
                    bx1 = mc1[i].BoundingBox().Max;

                    Autodesk.Navisworks.Api.Point3D Max =
                        new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048));

                    Autodesk.Navisworks.Api.Point3D Min =
                        new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048));

                        BoundingBox3D BBP = new BoundingBox3D(Min, Max);

                        if (BBP.Intersects(intrsct))
                        {
                            im.Add(BBP.Intersect(intrsct));
                            ml.Add(i);
                        }
                }

                List<BoundingBox3D> wk = new List<BoundingBox3D>(mc2.Count);
                List<int> kl = new List<int>(mc2.Count);

                for (i = 0; i < mc2.Count; i++)
                {
                    bn1 = mc2[i].BoundingBox().Min;
                    bx1 = mc2[i].BoundingBox().Max;

                    Autodesk.Navisworks.Api.Point3D Max =
                        new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048));

                    Autodesk.Navisworks.Api.Point3D Min =
                        new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048));

                    BoundingBox3D BBP = new BoundingBox3D(Min.Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D()), Max.Add(Micp[i2[i]].Protrusion.Max.ToVector3D()));

                    if (BBP.Intersects(intrsct))
                    {
                        kl.Add(i2[i]);
                            double mw = (date - SS[i2[i]]).Days * Micp[i2[i]].Rate;

                            if (Micp[i2[i]].Direction == ObjectItem.direction.Right)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X + mw, Min.Y, Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Min.X + mw + Micp[i2[i]].Rate, Max.Y, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Left)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Max.X - (mw + Micp[i2[i]].Rate), Min.Y, Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X - mw, Max.Y, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Up)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y + mw, Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Min.Y + mw + Micp[i2[i]].Rate, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Down)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Max.Y - (mw + Micp[i2[i]].Rate), Min.Z).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y - mw, Max.Z).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else if (Micp[i2[i]].Direction == ObjectItem.direction.Front)
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y, Min.Z + mw).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y, Min.Z + mw + Micp[i2[i]].Rate).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            else
                            {
                                Min = new Autodesk.Navisworks.Api.Point3D(Min.X, Min.Y, Max.Z - (mw + Micp[i2[i]].Rate)).Subtract(Micp[i2[i]].Protrusion.Min.ToVector3D());
                                Max = new Autodesk.Navisworks.Api.Point3D(Max.X, Max.Y, Max.Z - mw).Add(Micp[i2[i]].Protrusion.Max.ToVector3D());
                            }

                            BBP = new BoundingBox3D(Min, Max); wk.Add(BBP.Intersect(intrsct));
                    }
                }

                int iks = 0; if (isD) { iks = si == 0 ? Items.Length : setI[si + 1] - setI[si]; }

                //if (doItemsIntersect(im, ik, dm, dk, ml, kl))
               // {
                    for (i = 0; i < im.Count; i++)
                    {
                        for (int n = 0; n < wk.Count; n++)
                        {
                            if (wk[n].Intersects(im[i]))
                            {
                                double vol = wk[n].Intersect(im[i]).Volume;
                                v = v + vol;

                                int ii = SI[kl[n]].PropertyCategories.ElementAt(SI[kl[n]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                                if (isD)
                                {
                                    int ij = mc1[ml[i]].PropertyCategories.ElementAt(mc1[ml[i]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                                    if (si == 0)
                                    {
                                        intfm[(ij * iks) + ii] += vol; intfm[(ii * iks) + ij] += vol;
                                    }

                                    else
                                    {
                                        int a = setI[si] - setI[1];
                                        intfm[((i1[ml[i]] - a) * iks) + (kl[n] - a)] += vol; intfm[((kl[n] - a) * iks) + (i1[ml[i]] - a)] += vol;
                                    }
                                }

                                else
                                {
                                        if (si == 0)
                                        {
                                            intsm[ii] += vol;
                                        }

                                        else
                                        {
                                            intsm[((kl[n] - (setI[si] - setI[1])) * SItems.Count) + ml[i]] += vol;
                                        }
                                }
                            }
                        }
                    }
               // }
            }

            return v;
        }

        /*private bool doItemsIntersect(List<BoundingBox3D> im, List<BoundingBox3D> ik, List<DateTime> dm, List<DateTime> dk, List<int> ml, List<int> kl)
        {
            double mx = im[0].Min.X; double my = im[0].Min.Y; double mz = im[0].Min.Z;
            double px = im[0].Max.X; double py = im[0].Max.Y; double pz = im[0].Max.Z;

            Autodesk.Navisworks.Api.Point3D bn1;
            Autodesk.Navisworks.Api.Point3D bx1;

            for (int i = 1; i < im.Count; i++)
            {
                bn1 = im[i].Min;
                bx1 = im[i].Max;

                mx = Math.Min(mx, bn1.X);
                my = Math.Min(my, bn1.Y);
                mz = Math.Min(mz, bn1.Z);
                px = Math.Max(px, bx1.X);
                py = Math.Max(py, bx1.Y);
                pz = Math.Max(pz, bx1.Z);
            }

            bn1 = new Autodesk.Navisworks.Api.Point3D(mx, my, mz);
            bx1 = new Autodesk.Navisworks.Api.Point3D(px, py, pz);

            BoundingBox3D bb1 = new BoundingBox3D(bn1, bx1);

            mx = ik[0].Min.X; my = ik[0].Min.Y; mz = ik[0].Min.Z;
            px = ik[0].Max.X; py = ik[0].Max.Y; pz = ik[0].Max.Z;

            for (int i = 1; i < ik.Count; i++)
            {
                bn1 = ik[i].Min;
                bx1 = ik[i].Max;

                mx = Math.Min(mx, bn1.X);
                my = Math.Min(my, bn1.Y);
                mz = Math.Min(mz, bn1.Z);
                px = Math.Max(px, bx1.X);
                py = Math.Max(py, bx1.Y);
                pz = Math.Max(pz, bx1.Z);
            }

            bn1 = new Autodesk.Navisworks.Api.Point3D(mx, my, mz);
            bx1 = new Autodesk.Navisworks.Api.Point3D(px, py, pz);

            BoundingBox3D bb2 = new BoundingBox3D(bn1, bx1);

            if (bb1.Intersects(bb2))
            {
                BoundingBox3D intrscted = bb1.Intersect(bb2);

                int imc = im.Count; int ikc = ik.Count;

                for (int i = 0; i < im.Count; i++)
                {
                    if (!im[i].Intersects(intrscted))
                    {
                        im.RemoveAt(i);
                        dm.RemoveAt(i);
                        ml.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < ik.Count; i++)
                {
                    if (!ik[i].Intersects(intrscted))
                    {
                        ik.RemoveAt(i);
                        dk.RemoveAt(i);
                        kl.RemoveAt(i);
                        i--;
                    }
                }

                if (im.Count == imc && ikc == ik.Count)
                {
                    return true;
                }

                return doItemsIntersect(im, ik, dm, dk, ml, kl); 
            }
                return false;
        } */

        public void LoadDocument(String fileName, String TLFile)
        {
                //If the user has selected a valid location, then tell DocumentControl to open the file
                //As DocumentCtrl is linked to ViewControl

            if (File.Exists(TLFile))
            {
                if (documentControl.Document.TryOpenFile(fileName))
                {
                    this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    documentControl.SetAsMainDocument();
                    load = true; Selection_Tree.setItems(); Tasks = new String[31];

                    using (StreamReader r = new StreamReader(TLFile))
                    {
                        Items = new String[31];
                        OSDates = new DateTime[31]; OEDates = new DateTime[31];

                        r.ReadLine(); String line; int i = 0;

                        while ((line = r.ReadLine()) != null)
                        {
                            String[] f = line.Split(',');

                            if (f[4] != "")
                            {
                                if (i > 30)
                                {
                                    Array.Resize(ref Items, i + 30);
                                    Array.Resize(ref OSDates, i + 30);
                                    Array.Resize(ref OEDates, i + 30);
                                    Array.Resize(ref Tasks, i + 30);
                                }

                                Tasks[i] = f[1];
                                Items[i] = f[10].Substring(6);
                                String[] d = f[4].Split('/');
                                int month = int.Parse(d[0]); int day = int.Parse(d[1]);
                                int year = int.Parse(d[2]);
                                OSDates[i] = new DateTime(year, month, day);
                                d = f[5].Split('/');
                                month = int.Parse(d[0]); day = int.Parse(d[1]);
                                year = int.Parse(d[2]);
                                OEDates[i] = new DateTime(year, month, day);
                                i++;
                            }
                        }

                        if (Items.Length > i)
                        {
                            Array.Resize(ref Items, i);
                            Array.Resize(ref OSDates, i);
                            Array.Resize(ref OEDates, i);
                            Array.Resize(ref Tasks, i);
                        }
                    }

                    Selection_Tree.setItemSets();
                    SavedItemCollection si = documentControl.Document.SelectionSets.Value;
                    setI = new int[Items.Length + 2]; SI = new ModelItem[0]; setI[0] = 0;
                    SS = new DateTime[0]; SE = new DateTime[0]; Micp = new ObjectItem[0];
                    bool de = true; String[] sis = new String[0]; int[] sin = new int[0];

                    if (!Directory.Exists(currentDirectory + "/Selections/" + fileName.Substring(fileName.LastIndexOf('\\') + 1)))
                    {
                        de = false;
                        Directory.CreateDirectory(currentDirectory + "/Selections/" + fileName.Substring(fileName.LastIndexOf('\\') + 1));
                        sis = new String[Items.Length];
                        sin = new int[Items.Length];
                    }
                    comboBox1.Items.Add("All"); comboBox1.Items.AddRange(Items);
                    object[][] val1 = new object[0][]; int sI = 0;
                    object[][] val2 = new object[0][];

                    for (int i = 0; i < Items.Length; i++)
                    {
                        int j = si.IndexOfDisplayName(Items[i]);

                        SelectionSource ss = documentControl.Document.SelectionSets.CreateSelectionSource(si[j]);
                        Search s = new Search(); s.Selection.SelectionSources.Add(ss);
                        ModelItemCollection slci = s.Selection.GetSelectedItems(documentControl.Document);
                        int vl = SI.Length;
                        Array.Resize(ref SI, vl + slci.Count);
                        slci.CopyTo(SI, vl);
                        setI[i + 2] = slci.Count;
                        if (!de)
                        {
                            int f = Array.BinarySearch(sis, 0, sI, Items[i]);
                            if (f < 0)
                            {
                                Array.Copy(sis, -f - 1, sis, -f, sI - (-f - 1)); sis[-f - 1] = Items[i];
                                Array.Copy(sin, -f - 1, sin, -f, sI - (-f - 1)); sin[-f - 1] = -1; sI++;
                            }
                            else sin[f]++;
                            StreamWriter sw;
                            if (f < 0)
                                sw = new StreamWriter(new FileStream(currentDirectory + "/Selections/" +
                                    fileName.Substring(fileName.LastIndexOf('\\') + 1) + "/" + Items[i] + ".dn", FileMode.CreateNew, FileAccess.Write));
                            else
                                sw = new StreamWriter(new FileStream(currentDirectory + "/Selections/" +
                                    fileName.Substring(fileName.LastIndexOf('\\') + 1) + "/" + Items[i] + sin[f] + ".dn", FileMode.CreateNew, FileAccess.Write));

                            using (sw)
                            {
                                sw.Write("0,0,0,0,0,0,Default," + OSDates[i].ToShortDateString() + "," + OEDates[i].ToShortDateString());
                                for (j = 1; j < slci.Count; j++)
                                {
                                    sw.Write(Environment.NewLine);
                                    sw.Write("0,0,0,0,0,0,Default," + OSDates[i].ToShortDateString() + "," + OEDates[i].ToShortDateString());
                                }
                                sw.Close();
                            }
                        }

                        Array.Resize(ref val1, vl + slci.Count);
                        Array.Resize(ref val2, vl + slci.Count);
                        Array.Resize(ref SS, vl + slci.Count);
                        Array.Resize(ref SE, vl + slci.Count);
                        Array.Resize(ref Micp, vl + slci.Count);

                        using (StreamReader sr = new StreamReader(currentDirectory + "/Selections/" +
                            fileName.Substring(fileName.LastIndexOf('\\') + 1) + "/" + Items[i] + ".dn"))
                        {
                            for (j = 0; j < slci.Count; j++)
                            {
                                String Line = sr.ReadLine(); String[] ver = Line.Split(','); String[] d = ver[7].Split('/'); String[] d1 = ver[8].Split('/');
                                SS[vl + j] = new DateTime(int.Parse(d[2]), int.Parse(d[0]), int.Parse(d[1]));
                                SE[vl + j] = new DateTime(int.Parse(d1[2]), int.Parse(d1[0]), int.Parse(d1[1]));
                                ObjectItem Item = new ObjectItem(0, ObjectItem.direction.Up, SS[vl + j], SE[vl + j],
                                    new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(double.Parse(ver[0]), double.Parse(ver[1]), double.Parse(ver[2])),
                                                      new Autodesk.Navisworks.Api.Point3D(double.Parse(ver[3]), double.Parse(ver[4]), double.Parse(ver[5]))));
                                Item.IsDynamic = true;
                                double X = SI[vl + j].BoundingBox().Size.X; double Y = SI[vl + j].BoundingBox().Size.Y;
                                double Z = SI[vl + j].BoundingBox().Size.Z; double r = 0.3048 / (SE[vl + j] - SS[vl + j]).TotalDays;
                                X = X * r; Y = Y * r; Z = Z * r;

                                if (ver[6] == "Default")
                                {
                                    if (X >= Y && X >= Z)
                                    {
                                        Item.Direction = ObjectItem.direction.Left;
                                        Item.Rate = X;

                                        if (Item.Protrusion.Size.IsZero)
                                        {
                                            Item.Protrusion = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(0, 1, 1),
                                                                                new Autodesk.Navisworks.Api.Point3D(0, 1, 1));
                                        }
                                    }
                                    else if (Y >= X && Y >= Z)
                                    {
                                        Item.Direction = ObjectItem.direction.Up;
                                        Item.Rate = Y;

                                        if (Item.Protrusion.Size.IsZero)
                                        {
                                            Item.Protrusion = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(1, 0, 1),
                                                                                new Autodesk.Navisworks.Api.Point3D(1, 0, 1));
                                        }
                                    }
                                    else
                                    {
                                        Item.Direction = ObjectItem.direction.Front;
                                        Item.Rate = Z;

                                        if (Item.Protrusion.Size.IsZero)
                                        {
                                            Item.Protrusion = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(1, 1, 0),
                                                                                new Autodesk.Navisworks.Api.Point3D(1, 1, 0));
                                        }
                                    }
                                }

                                else
                                {
                                    Item.Direction = ObjectItem.getDirection(ver[6]);
                                    if (ver[6] == "Left" || ver[6] == "Right")
                                        Item.Rate = X;
                                    if (ver[6] == "Up" || ver[6] == "Down")
                                        Item.Rate = Y;
                                    if (ver[6] == "Front" || ver[6] == "Back")
                                        Item.Rate = Z;
                                }

                                Micp[vl + j] = Item;

                                val1[vl + j] = new object[3]; val2[vl + j] = new object[6];

                                val1[vl + j][0] = Tasks[i]; val1[vl + j][1] = SS[vl + j]; val1[vl + j][2] = SE[vl + j];

                                val2[vl + j][0] = Item.Rate; val2[vl + j][1] = Item.Direction.ToString(); val2[vl + j][4] = i; val2[vl + j][5] = j;
                                val2[vl + j][2] = Item.Protrusion.Min.ToString().Substring(1, Item.Protrusion.Min.ToString().Length - 2);
                                val2[vl + j][3] = Item.Protrusion.Max.ToString().Substring(1, Item.Protrusion.Max.ToString().Length - 2);
                            }
                            sr.Close();
                        }
                    }
                    OSEDates = Clone(OEDates); Array.Sort(OSEDates);
                    documentControl.Document.Models.SetHidden(SI, true); SItems = new ModelItemCollection();

                    getStaticItems(documentControl.Document.Models.RootItems); SItems.Minimize();

                    documentControl.Document.Models.SetHidden(SItems, true);

                    String[] properties = { "Contained in Task", "Start", "End" };
                    String[] Types = { "System.String", "System.DateTime", "System.DateTime" };
                    String Category = "TimeLiner";

                    addProperty(SI, Category, properties, val1, Types);

                    properties = new[] { "Rate", "Direction", "Min Protrusion", "Max Protrusion", "SI", "I of I in S" };
                    Types = new[] { "System.String", "System.String", "Autodesk.Navisworks.Api.Point3D",
                    "Autodesk.Navisworks.Api.Point3D", "System.int32", "System.int32"};
                    Category = "Dynamics";

                    addProperty(SI, Category, properties, val2, Types);

                    sT = new int[SI.Length * 2]; edp = new int[SI.Length * 2]; setI[1] = SI.Length;

                    int k;
                    for (k = 2; k < setI.Length; k++) { setI[k] = setI[k] + setI[k - 1]; }

                    for (k = 0; k < setI[1]; k++) { sT[k] = k; edp[k] = k; }

                    Array.Sort(Clone(SE), edp, 0, setI[1]); Array.Sort(Clone(SS), sT, 0, setI[1]);
                    int[] ip = new int[Items.Length]; int[] ie = new int[Items.Length];

                    for (k = 0; k < setI[1]; k++)
                    {
                        //int i = SI[sT[k]].PropertyCategories.ElementAt(SI[sT[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                        //int j = SI[edp[k]].PropertyCategories.ElementAt(SI[edp[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                        int i = 1; while (setI[i] - (sT[k] + setI[1]) <= 0) { i++; } i--;
                        int j = 1; while (setI[j] - (edp[k] + setI[1]) <= 0) { j++; } j--;

                        sT[setI[i] + ip[i - 1]] = sT[k]; edp[setI[j] + ie[j - 1]] = edp[k];

                        ip[i - 1]++; ie[j - 1]++;
                    }

                    timeSlider1.Maximum = SE[edp[setI[1] - 1]].AddDays(1); timeSlider1.Minimum = SS[sT[0]].AddDays(-1);
                    timeSlider1.Value = timeSlider1.Minimum;

                    glControl1.MakeCurrent(); GL.ClearColor(System.Drawing.Color.SkyBlue); SetupViewport(); glControl1.Invalidate();

                    this.Cursor = System.Windows.Forms.Cursors.Default;
                }
            }

            else
            {
                if (MessageBox.Show(this, "Would you like to locate the TimeLiner yourself?", "TimeLiner does not exist in same directory as the opened file or its name is different", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    OpenFileDialog dlg = new OpenFileDialog();
                    dlg.CheckFileExists = true;
                    dlg.CheckPathExists = true;
                    dlg.DefaultExt = "csv";
                    dlg.Filter = "CSV (Comma delimited) (*.csv)|*.csv|All files (*.*)|*.*";

                    //Ask user for file location
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        LoadDocument(fileName, dlg.FileName);
                    }
                }
            }
        }

        private void getStaticItems(ModelItemEnumerableCollection modelItems)
        {
            foreach (ModelItem m in modelItems)
            {
                if (!m.IsHidden)
                {
                    if (m.HasGeometry)
                    {
                       SItems.Add(m);
                    }

                    else
                    {
                        getStaticItems(m.Children);
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void change_itemProperties(int i, int j)
        {
            int k = j == -1 ? 0 : j;

            int n = setI[i + 1] - setI[1];

            Change_Properties CP = new Change_Properties(Micp[n + k].Protrusion.Max.X, Micp[n + k].Protrusion.Min.X,
                                                         Micp[n + k].Protrusion.Max.Y, Micp[n + k].Protrusion.Min.Y,
                                                         Micp[n + k].Protrusion.Max.Z, Micp[n + k].Protrusion.Min.Z, 
                                                         OSDates[i], OEDates[i], false);

            if (CP.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                if (j == -1)
                {
                    String[] properties = { "Contained in Task", "Start", "End" };
                    String[] Types = { "System.String", "System.DateTime", "System.DateTime" };
                    String Category = "TimeLiner";

                    int si = setI[i + 2] - setI[i + 1];

                    object[][] val1 = new object[si][]; ModelItem[] mt = new ModelItem[si];

                    for (int d = 0; d < si; d++)
                    {
                        val1[d] = new object[3];
                        Micp[n + d].StartDate = CP.StartDate; Micp[n + d].EndDate = CP.EndDate;
                        SS[n + d] = CP.StartDate; SE[n + d] = CP.EndDate;

                        val1[d][0] = Tasks[i]; val1[d][1] = SS[n + d]; val1[d][2] = SE[n + d];
                        mt[d] = SI[n + d];
                    }

                    addProperty(mt, Category, properties, val1, Types);

                    properties = new[] { "Rate", "Direction", "Min Protrusion", "Max Protrusion", "SI", "I of I in S" };
                    Types = new[] { "System.String", "System.String", "Autodesk.Navisworks.Api.Point3D",
                    "Autodesk.Navisworks.Api.Point3D", "System.int32", "System.int32"};
                    Category = "Dynamics";

                    for (int d = 0; d < si; d++)
                    {
                        val1[d] = new object[6];

                        double X = mt[d].BoundingBox().Size.X; double Y = mt[d].BoundingBox().Size.Y;
                        double Z = mt[d].BoundingBox().Size.Z; double r = 0.3048 / (SE[n + d] - SS[n + d]).TotalDays;
                        X = X * r; Y = Y * r; Z = Z * r;

                        Micp[n + d].Direction = CP.D;
                        if (CP.D == ObjectItem.direction.Left || CP.D == ObjectItem.direction.Right)
                            Micp[n + d].Rate = X;
                        if (CP.D == ObjectItem.direction.Up || CP.D == ObjectItem.direction.Down)
                            Micp[n + d].Rate = Y;
                        if (CP.D == ObjectItem.direction.Front || CP.D == ObjectItem.direction.Back)
                            Micp[n + d].Rate = Z;

                        val1[d][0] = Micp[n + d].Rate; val1[d][1] = Micp[n + d].Direction.ToString();
                        Micp[n + d].Protrusion = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(CP.MX, CP.MY, CP.MZ),
                                                                  new Autodesk.Navisworks.Api.Point3D(CP.PX, CP.PY, CP.PZ));
                        val1[d][2] = Micp[n + d].Protrusion.Min.ToString().Substring(1, Micp[n + d].Protrusion.Min.ToString().Length - 2);
                        val1[d][3] = Micp[n + d].Protrusion.Max.ToString().Substring(1, Micp[n + d].Protrusion.Max.ToString().Length - 2);
                        val1[d][4] = i; val1[d][5] = d;
                    }

                    addProperty(mt, Category, properties, val1, Types);

                    StreamWriter sw = new StreamWriter(new FileStream(currentDirectory + "/Selections/" +
       documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[i] + ".dn", FileMode.Truncate, FileAccess.Write));

                    using (sw)
                    {
                        sw.Write(CP.MX + "," + CP.MY + "," + CP.MZ + "," + CP.PX + "," + CP.PY + "," + CP.PZ + "," + CP.D.ToString() + "," +
                            CP.StartDate.ToShortDateString() + "," + CP.EndDate.ToShortDateString());
                        for (j = 1; j < si; j++)
                        {
                            sw.Write(Environment.NewLine);
                            sw.Write(CP.MX + "," + CP.MY + "," + CP.MZ + "," + CP.PX + "," + CP.PY + "," + CP.PZ + "," + CP.D.ToString() + "," +
                            CP.StartDate.ToShortDateString() + "," + CP.EndDate.ToShortDateString());
                        }
                        sw.Close();
                    }
                }

                else
                {
                    String[] properties = { "Contained in Task", "Start", "End" };
                    String[] Types = { "System.String", "System.DateTime", "System.DateTime" };
                    String Category = "TimeLiner";

                    object[][] val1 = new object[1][];

                    val1[0] = new object[3];
                    Micp[n + j].StartDate = CP.StartDate; Micp[n + j].EndDate = CP.EndDate;
                    SS[n + j] = CP.StartDate; SE[n + j] = CP.EndDate;

                    val1[0][0] = Tasks[i]; val1[0][1] = SS[n + j]; val1[0][2] = SE[n + j];

                    addProperty(SI[n + j].Self.ToArray(), Category, properties, val1, Types);

                    properties = new[] { "Rate", "Direction", "Min Protrusion", "Max Protrusion", "SI", "I of I in S" };
                    Types = new[] { "System.String", "System.String", "Autodesk.Navisworks.Api.Point3D",
                    "Autodesk.Navisworks.Api.Point3D", "System.int32", "System.int32"};
                    Category = "Dynamics";

                    val1[0] = new object[6];

                    double X = SI[n + j].BoundingBox().Size.X; double Y = SI[n + j].BoundingBox().Size.Y;
                    double Z = SI[n + j].BoundingBox().Size.Z; double r = 0.3048 / (SE[n + j] - SS[n + j]).TotalDays;
                    X = X * r; Y = Y * r; Z = Z * r;

                    Micp[n + j].Direction = CP.D;
                    if (CP.D == ObjectItem.direction.Left || CP.D == ObjectItem.direction.Right)
                        Micp[n + j].Rate = X;
                    if (CP.D == ObjectItem.direction.Up || CP.D == ObjectItem.direction.Down)
                        Micp[n + j].Rate = Y;
                    if (CP.D == ObjectItem.direction.Front || CP.D == ObjectItem.direction.Back)
                        Micp[n + j].Rate = Z;

                    val1[0][0] = Micp[n + j].Rate; val1[0][1] = Micp[n + j].Direction.ToString();
                    Micp[n + j].Protrusion = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(CP.MX, CP.MY, CP.MZ),
                                                              new Autodesk.Navisworks.Api.Point3D(CP.PX, CP.PY, CP.PZ));
                    val1[0][2] = Micp[n + j].Protrusion.Min.ToString().Substring(1, Micp[n + j].Protrusion.Min.ToString().Length - 2);
                    val1[0][3] = Micp[n + j].Protrusion.Max.ToString().Substring(1, Micp[n + j].Protrusion.Max.ToString().Length - 2);
                    val1[0][4] = i; val1[0][5] = j;
                    addProperty(SI[n + j].Self.ToArray(), Category, properties, val1, Types);

                    String[] lines = File.ReadAllLines(currentDirectory + "/Selections/" +
       documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[i] + ".dn");

                    lines[j] = CP.MX + "," + CP.MY + "," + CP.MZ + "," + CP.PX + "," + CP.PY + "," + CP.PZ + "," + CP.D.ToString() + "," +
                            CP.StartDate.ToShortDateString() + "," + CP.EndDate.ToShortDateString();

                    File.WriteAllLines(currentDirectory + "/Selections/" +
                           documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[i] + ".dn", lines);

                    Properties.displayProperties(SI[n + j].PropertyCategories);
                }

                for (k = 0; k < setI[1]; k++) { sT[k] = k; edp[k] = k; }

                Array.Sort(Clone(SE), edp, 0, setI[1]); Array.Sort(Clone(SS), sT, 0, setI[1]);
                int[] ip = new int[Items.Length]; int[] ie = new int[Items.Length];

                for (k = 0; k < setI[1]; k++)
                {
                    //int i = SI[sT[k]].PropertyCategories.ElementAt(SI[sT[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                    //int j = SI[edp[k]].PropertyCategories.ElementAt(SI[edp[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                    i = 1; while (setI[i] - (sT[k] + setI[1]) <= 0) { i++; } i--;
                    j = 1; while (setI[j] - (edp[k] + setI[1]) <= 0) { j++; } j--;

                    sT[setI[i] + ip[i - 1]] = sT[k]; edp[setI[j] + ie[j - 1]] = edp[k];

                    ip[i - 1]++; ie[j - 1]++;
                }

                comboBox1.SelectedIndex = -1; LineChart.Series[0].Points.Clear();
                precalc = false;
                timeSlider1_Scroll(this, EventArgs.Empty);
                button1.Enabled = true;
            }
        }

        public void change_itemProperties(int[] i, int [] j)
        {
            Change_Properties CP = new Change_Properties(0, 0, 0, 0, 0, 0, OSDates[0], OSEDates[OEDates.Length - 1], true);

            if (CP.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                    String[] properties = { "Contained in Task", "Start", "End" };
                    String[] Types = { "System.String", "System.DateTime", "System.DateTime" };
                    String Category = "TimeLiner";

                    object[][] val1 = new object[i.Length][]; ModelItem[] ij = new ModelItem[i.Length];

                    int k, n;
                    for (k = 0, n = 0; k < i.Length; k++, n = 0)
                    {
                        val1[k] = new object[3];

                        n = setI[i[k] + 1] - setI[1];

                        ij[k] = SI[n + j[k]]; Micp[n + j[k]].StartDate = CP.StartDate; Micp[n + j[k]].EndDate = CP.EndDate;
                        SS[n + j[k]] = CP.StartDate; SE[n + j[k]] = CP.EndDate;

                        val1[k][0] = Tasks[i[k]]; val1[k][1] = SS[n + j[k]]; val1[k][2] = SE[n + j[k]];
                    }

                    addProperty(ij, Category, properties, val1, Types);

                    properties = new[] { "Rate", "Direction", "Min Protrusion", "Max Protrusion", "SI", "I of I in S" };
                    Types = new[] { "System.String", "System.String", "Autodesk.Navisworks.Api.Point3D",
                    "Autodesk.Navisworks.Api.Point3D", "System.int32", "System.int32"};
                    Category = "Dynamics";

                    for (k = 0, n = 0; k < i.Length; k++, n = 0)
                    {
                        val1[k] = new object[6];

                        n = setI[i[k] + 1] - setI[1];

                        double X = ij[k].BoundingBox().Size.X; double Y = ij[k].BoundingBox().Size.Y;
                        double Z = ij[k].BoundingBox().Size.Z; double r = 0.3048 / (SE[n + j[k]] - SS[n + j[k]]).TotalDays;
                        X = X * r; Y = Y * r; Z = Z * r;

                        Micp[n + j[k]].Direction = CP.D;
                        if (CP.D == ObjectItem.direction.Left || CP.D == ObjectItem.direction.Right)
                            Micp[n + j[k]].Rate = X;
                        if (CP.D == ObjectItem.direction.Up || CP.D == ObjectItem.direction.Down)
                            Micp[n + j[k]].Rate = Y;
                        if (CP.D == ObjectItem.direction.Front || CP.D == ObjectItem.direction.Back)
                            Micp[n + j[k]].Rate = Z;

                        val1[k][0] = Micp[n + j[k]].Rate; val1[k][1] = Micp[n + j[k]].Direction.ToString();
                        Micp[n + j[k]].Protrusion = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(CP.MX, CP.MY, CP.MZ),
                                                                  new Autodesk.Navisworks.Api.Point3D(CP.PX, CP.PY, CP.PZ));
                        val1[k][2] = Micp[n + j[k]].Protrusion.Min.ToString().Substring(1, Micp[n + j[k]].Protrusion.Min.ToString().Length - 2);
                        val1[k][3] = Micp[n + j[k]].Protrusion.Max.ToString().Substring(1, Micp[n + j[k]].Protrusion.Max.ToString().Length - 2);
                        val1[k][4] = i[k]; val1[k][5] = j[k];
                    }
                    addProperty(ij, Category, properties, val1, Types);
                    Array.Sort(i, j); 

                    for (k = 0; k < i.Length; k++)
                    {
                        String[] lines = File.ReadAllLines(currentDirectory + "/Selections/" +
                        documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[i[k]] + ".dn");

                        do
                        {
                            lines[j[k]] = CP.MX + "," + CP.MY + "," + CP.MZ + "," + CP.PX + "," + CP.PY + "," + CP.PZ + "," + CP.D.ToString() + "," +
                                    CP.StartDate.ToShortDateString() + "," + CP.EndDate.ToShortDateString();
                            k++;
                        }
                        while (k < i.Length && i[k] == i[k - 1]); k--;

                        File.WriteAllLines(currentDirectory + "/Selections/" +
                               documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[i[k]] + ".dn", lines);
                    }

                    for (k = 0; k < setI[1]; k++) { sT[k] = k; edp[k] = k; }

                    Array.Sort(Clone(SE), edp, 0, setI[1]); Array.Sort(Clone(SS), sT, 0, setI[1]);
                    int[] ip = new int[Items.Length]; int[] ie = new int[Items.Length];

                    for (k = 0; k < setI[1]; k++)
                    {
                        //n = SI[sT[k]].PropertyCategories.ElementAt(SI[sT[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                        //int m = SI[edp[k]].PropertyCategories.ElementAt(SI[edp[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                            n = 1; while (setI[n] - (sT[k] + setI[1]) <= 0) { n++; } n--;
                        int m = 1; while (setI[m] - (edp[k] + setI[1]) <= 0) { m++; } m--;

                        sT[setI[n] + ip[n - 1]] = sT[k]; edp[setI[m] + ie[m - 1]] = edp[k];

                        ip[n - 1]++; ie[m - 1]++;
                    }

                    comboBox1.SelectedIndex = -1; LineChart.Series[0].Points.Clear();
                    precalc = false;
                    timeSlider1_Scroll(this, EventArgs.Empty);
                    button1.Enabled = true; 
            }
        }

        public void timeSlider1_Scroll(object sender, EventArgs e)
        {
            if (!load) return;

            documentControl.Document.Models.SetHidden(SI, true);
            documentControl.Document.Models.SetHidden(SItems, true);
            documentControl.Document.Models.ResetPermanentMaterials(SI);
            documentControl.Document.Models.ResetPermanentMaterials(SItems);

            if (precalc)
            {
                int y = 0;
                while (y < ADates.Length && ADates[y] <= timeSlider1.Value)
                {
                    y++;
                }

                if (y > 0)
                {
                    for (int i = 0; i < ISMIE[y - 1].Length; i++)
                    {
                        if (!ISMIE[y - 1][i])
                        {
                            documentControl.Document.Models.OverridePermanentColor(SI[i].Self, Autodesk.Navisworks.Api.Color.FromByteRGB(255, 255, 0));
                        }
                    }
                    documentControl.Document.Models.SetHidden(mics[y - 1], false);

                    if (y == ADates.Length)
                    {
                        documentControl.Document.Models.SetHidden(SItems, false);

                    }
                }
            }
            else
            {
                setVisible();
            }

            glControl1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (load)
            {
                preCalculate();
                precalc = true;
                button1.Enabled = false;
            }
        }

        private void SetupViewport()
        {
            int w = glControl1.Width;
            int h = glControl1.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Ortho(-160, 160, -120.0, 120.0, -50, 50);
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }

        private void modelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayControl.Controls.Clear(); 
            displayControl.Controls.Add(this.timeSlider1); displayControl.Controls.Add(this.button1);
            displayControl.Controls.Add(viewControl);
        }

        private void chartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayControl.Controls.Clear();
            LineChart.Enabled = true;
            LineChart.Titles[0].Text = "Intersected Volume";
            LineChart.ChartAreas[0].AxisY.Title = "Volume (m3)";
            LineChart.ChartAreas[0].AxisX.Title = "Time (per day)";
            LineChart.Series[0].XValueType = ChartValueType.DateTime;
            LineChart.ChartAreas[0].CursorX.IsUserEnabled = true;
            LineChart.ChartAreas[0].CursorY.IsUserEnabled = true;
            displayControl.Controls.Add(LineChart); LineChart.Controls.Clear(); 
            LineChart.Controls.Add(this.comboBox1);
            updateChart(comboBox1.SelectedIndex);
        }

        private void boundingBoxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayControl.Controls.Clear();
            displayControl.Controls.Add(this.timeSlider1); displayControl.Controls.Add(this.button1);
            displayControl.Controls.Add(glControl1);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (load)
            {
                if (precalc)
                {
                    int y = 0;

                    while (y < ADates.Length && ADates[y] <= timeSlider1.Value)
                    {
                        y++;
                    }

                    if (y > 0)
                    {
                        for (int i = 0; i < ISMIE[y - 1].Length; i++)
                        {
                            System.Drawing.Color color;

                            if (!ISMIE[y - 1][i])
                            {
                                color = System.Drawing.Color.Yellow;
                            }
                            else
                            {
                                color = System.Drawing.Color.Gray;
                            }

                            Autodesk.Navisworks.Api.Point3D bn1 = SI[i].BoundingBox().Min;
                            Autodesk.Navisworks.Api.Point3D bx1 = SI[i].BoundingBox().Max;

                            BoundingBox3D bb1 = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(bn1.X, bn1.Y, bn1.Z),
                                                                      new Autodesk.Navisworks.Api.Point3D(bx1.X, bx1.Y, bx1.Z));

                            Draw3DRec(bb1.Min, bb1.Max, color);
                        }

                        if (y == ADates.Length)
                        {
                            for (int j = 0; j < SItems.Count; j++)
                            {
                                Autodesk.Navisworks.Api.Point3D bn1 = SItems[j].BoundingBox().Min;
                                Autodesk.Navisworks.Api.Point3D bx1 = SItems[j].BoundingBox().Max;

                                BoundingBox3D bb1 = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(bn1.X, bn1.Y, bn1.Z),
                                                                      new Autodesk.Navisworks.Api.Point3D(bx1.X, bx1.Y, bx1.Z));

                                Draw3DRec(bb1.Min, bb1.Max, System.Drawing.Color.Gray);
                            }
                        }
                    }
                }
                else
                {
                    setBoundingBoxesVisible();
                }

                if (Selection_Tree.ModelSelection.Count > 0)
                {
                    ModelItemCollection vs = new ModelItemCollection();

                    getLastVisibleChildren(Selection_Tree.ModelSelection, vs);

                    for (int k = 0; k < vs.Count; k++)
                    {
                        ModelItem model = vs[k];
                        Autodesk.Navisworks.Api.Point3D bn1 = model.BoundingBox().Min;
                        Autodesk.Navisworks.Api.Point3D bx1 = model.BoundingBox().Max;

                        BoundingBox3D bb1 = new BoundingBox3D(new Autodesk.Navisworks.Api.Point3D(bn1.X, bn1.Y, bn1.Z),
                                                              new Autodesk.Navisworks.Api.Point3D(bx1.X, bx1.Y, bx1.Z));

                        Draw3DRec(bb1.Min, bb1.Max, System.Drawing.Color.Blue);
                    }
                }
            }
                glControl1.SwapBuffers();
      }

        public void getLastVisibleChildren(ModelItemCollection selec, ModelItemCollection vs)
        {
            for (int i = 0; i < selec.Count; i++)
            {
                if (!selec[i].IsHidden)
                {
                    if (selec[i].HasGeometry)
                    {
                        vs.Add(selec.ElementAt(i));
                    }

                    else
                        getLastVisibleChildren(toModelItemCollection(selec[i].Children), vs);
                }
            }
        }

        public ModelItemCollection toModelItemCollection(ModelItemEnumerableCollection MIEC)
        {
            ModelItemCollection MIC = new ModelItemCollection(); MIC.AddRange(MIEC);

            return MIC;
        }

        public void Draw3DRec(Autodesk.Navisworks.Api.Point3D Min, Autodesk.Navisworks.Api.Point3D Max, System.Drawing.Color color) // draws the targets
        {
            GL.PushMatrix();
            GL.Rotate(-25, 0, 1, 0);
            GL.Rotate(25, 1, 0, 0);
            GL.Scale(1, 1, 1);

            GL.Color3(color);
            
            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(Min.X, Min.Y, Min.Z);
            GL.Vertex3(Min.X, Min.Y, Max.Z);
            GL.Vertex3(Min.X, Max.Y, Max.Z);
            GL.Vertex3(Min.X, Max.Y, Min.Z);
            GL.End();

            GL.Begin(BeginMode.Lines);
            GL.Vertex3(Min.X, Min.Y, Min.Z);
            GL.Vertex3(Max.X, Min.Y, Min.Z);
            GL.Vertex3(Min.X, Min.Y, Max.Z);
            GL.Vertex3(Max.X, Min.Y, Max.Z);
            GL.Vertex3(Min.X, Max.Y, Max.Z);
            GL.Vertex3(Max.X, Max.Y, Max.Z);
            GL.Vertex3(Min.X, Max.Y, Min.Z);
            GL.Vertex3(Max.X, Max.Y, Min.Z);
            GL.End();

            GL.Begin(BeginMode.LineLoop);
            GL.Vertex3(Max.X, Min.Y, Min.Z);
            GL.Vertex3(Max.X, Min.Y, Max.Z);
            GL.Vertex3(Max.X, Max.Y, Max.Z);
            GL.Vertex3(Max.X, Max.Y, Min.Z);
            GL.End();

            GL.PopMatrix();
        }

        private void exportChartDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV (Comma delimited) (*.csv)|*.csv";
            saveFileDialog1.CreatePrompt = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.FileName = "Untitled.csv";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    int c = LineChart.Series[0].Points.Count;

                    for (int i = 0; i < c; i++)
                    {
                        sw.WriteLine(DateTime.FromOADate(LineChart.Series[0].Points[i].XValue) + "," + Math.Round(LineChart.Series[0].Points[i].YValues[0], 2));
                    }

                    sw.Close();
                }
            }
        }

        private void addProperty(ModelItem[] MICIn, String Category, String[] Properties, object[][] values, String[] types)
        {
            InwOpState10 state;

            state = ComApiBridge.State;

            for (int i = 0; i < MICIn.Length; i++)
            {
                ModelItemEnumerableCollection mie = MICIn[i].DescendantsAndSelf;

                for (int k = 0; k < mie.Count(); k++)
                {
                    InwOaPath3 oPath = (InwOaPath3)Autodesk.Navisworks.Api.ComApi.ComApiBridge.ToInwOaPath(mie.ElementAt(k));

                    Autodesk.Navisworks.Api.Interop.ComApi.InwGUIPropertyNode2 propn = (InwGUIPropertyNode2)state.GetGUIPropertyNode(oPath, true);

                    if (Category == "TimeLiner")
                    {
                       propn.RemoveUserDefined(0);
                    }

                    InwOaPropertyVec newPvec = (InwOaPropertyVec)state.ObjectFactory(Autodesk.Navisworks.Api.Interop.ComApi.nwEObjectType.eObjectType_nwOaPropertyVec, null, null);

                    for (int j = 0; j < Properties.Length; j++)
                    {
                        InwOaProperty newP = (InwOaProperty)state.ObjectFactory(Autodesk.Navisworks.Api.Interop.ComApi.nwEObjectType.eObjectType_nwOaProperty, Type.GetType(types[j]), null);

                        newP.name = Properties[j].Substring(0, 2);

                        newP.UserName = Properties[j];

                        newP.value = values[i][j];

                        newPvec.Properties().Add(newP);
                    }
                        propn.SetUserDefined(0, Category, Category.Substring(0, 2), newPvec);
                }
            }
        }

        private void selectionTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Selection_Tree.IsHidden)
            {
                Selection_Tree.Show();
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.IsHidden)
            {
                Properties.Show();
            }
        }

        private void LineChart_MouseClick(object sender, MouseEventArgs e)
        {
            if (LineChart.Series[0].Points.Count == 0 || !LineChart.ChartAreas[0].CursorX.IsUserEnabled) return;
            
            double x = e.X / (double)LineChart.Width * 100;
            double y = e.Y / (double)LineChart.Height * 100;

            if (x >= LineChart.ChartAreas[0].Position.X && x <= LineChart.ChartAreas[0].Position.Right
             && y >= LineChart.ChartAreas[0].Position.Y && y <= LineChart.ChartAreas[0].Position.Bottom)
            {
                ElementPosition ep = LineChart.ChartAreas[0].InnerPlotPosition;

                x = ((x - LineChart.ChartAreas[0].Position.X) / LineChart.ChartAreas[0].Position.Width) * 100;
                y = ((y - LineChart.ChartAreas[0].Position.Y) / LineChart.ChartAreas[0].Position.Height) * 100;

                if (x >= ep.X && x <= ep.Right && y >= ep.Y && y <= ep.Bottom)
                {
                    this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
                    int si = comboBox1.SelectedIndex;
                    LineChart.ChartAreas[0].CursorX.SetCursorPixelPosition(new Point(e.X, e.Y), false);
                    DateTime dateTime = DateTime.FromOADate(LineChart.ChartAreas[0].CursorX.Position);
                    DataPoint dataPoint = LineChart.Series[0].Points.FindByValue(LineChart.ChartAreas[0].CursorX.Position, "X");
                    String IntersectedSets = "";  //getIntersectedItems(SI, dateTime);

                    int ij = (dateTime - SS[sT[setI[si]]]).Days + 1;

                    if(ij >= 0 && intfm[ij] != null)
                    {
                        if (si == 0)
                        {
                            for (int i = 0; i < Items.Length; i++)
                            {
                                for (int j = i + 1; j < Items.Length; j++)
                                {
                                    if (intfm[ij][i * Items.Length + j] > 0)
                                    {
                                        IntersectedSets += Items[i] + ", " + Items[j] + " " + Math.Round(intfm[ij][i * Items.Length + j], 2) + "\r\n";
                                    }
                                }
                            }

                            for (int i = 0; i < Items.Length; i++)
                            {
                                if (intsm[ij][i] > 0)
                                {
                                    IntersectedSets += "Static Items" + ", " + Items[i] + " " + Math.Round(intsm[ij][i], 2) + "\r\n";
                                }
                            }
                        }

                        else
                        {
                            int l = setI[si + 1] - setI[si];

                            for (int i = 0; i < l; i++)
                            {
                                for (int j = i + 1; j < l; j++)
                                {
                                    if (intfm[ij][i * l + j] > 0)
                                    {
                                        String s1 = SI[sT[setI[si] + i]].PropertyCategories.ElementAt(0).Properties.ElementAt(0).Value.ToDisplayString();
                                        String s2 = SI[sT[setI[si] + j]].PropertyCategories.ElementAt(0).Properties.ElementAt(0).Value.ToDisplayString();
                                        IntersectedSets += s1 + ", " + s2 + " " + Math.Round(intfm[ij][i * l + j], 2) + "\r\n";
                                    }
                                }
                            }

                            for (int i = 0; i < l; i++)
                            {
                                for (int j = 0; j < SItems.Count; j++)
                                {
                                    if (intsm[ij][i * SItems.Count + j] > 0)
                                    {
                                        String s1 = SI[sT[setI[si] + i]].PropertyCategories.ElementAt(0).Properties.ElementAt(0).Value.ToDisplayString();
                                        String s2 = SItems[j].PropertyCategories.ElementAt(0).Properties.ElementAt(0).Value.ToDisplayString();
                                        IntersectedSets += s1 + ", " + s2 + " " + Math.Round(intsm[ij][i * SItems.Count + j], 2) + "\r\n";
                                    }
                                }
                            }
                        }
                    }

                    IntersectedItems ii = new IntersectedItems();
                    ii.Text = si == 0 ? "Intersected Sets" : "Intersected Items";
                    ii.TextBox.Text = "Date: " + dateTime.ToShortDateString() + ", Volume: " +
                        (dataPoint == null ? "0.00" : Math.Round(dataPoint.YValues[0], 2).ToString()) + "\r\n" +
                        (si == 0 ? "Sets:\r\n" : "Items:\r\n") + IntersectedSets;
                    ii.ShowDialog(this);
                    this.Cursor = System.Windows.Forms.Cursors.Default;
                }

                else
                {
                    LineChart.ChartAreas[0].CursorX.Position = 0;
                }
            }

            else
            {
                LineChart.ChartAreas[0].CursorX.Position = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateChart(comboBox1.SelectedIndex);
        }

        private void updateChart(int si)
        {
            LineChart.Series[0].Points.Clear(); // clears all point in the coollection before updating with the new data
            
            if (si == -1 || setI[si + 1] - setI[si] == 0) return;

            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

            DateTime start = SS[sT[setI[si]]]; DateTime end = SE[edp[setI[si + 1] - 1]];
            LineChart.ChartAreas[0].AxisX.Interval = (end - start).Days / 30;
            int d = (end - start).Days;
            Double[] Vol = new double[d + 2]; DateTime[]
                 dts = new DateTime[d + 2]; Vol[0] = 0;
            intfm = new double[d + 2][]; intsm = new double[d + 2][];
            int k = 1;  dts[0] = start.AddDays(-1);

            for (DateTime dt = start; dt <= end; dt = dt.AddDays(1), k++)
            {
                dts[k] = dt;

                Vol[k] = getIntersectsVolume(si, dts[k], ref intfm[k], ref intsm[k]);
            }

            addData(Vol, dts);

            this.Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void minimiseTheMaximumVolumeGeneticlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!load) return;

            int[] durations = new int[OSDates.Length]; 
            
            for (int i = 0; i < durations.Length; i++) 
            {
                durations[i] = (OEDates[i] - OSDates[i]).Days;
            }


            Parameters pr = new Parameters(durations);

            String[] ai = new String[Items.Length + 1]; ai[0] = "All"; Items.CopyTo(ai, 1);

            pr.setSetItems(ai); pr.setSelecMethods(new String[] { "Elite Selection", "Rank Selection", "Roulette Wheel Selection" });

            if (pr.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                int s = pr.getSelectedSetIndex;
                if (setI[s + 1] - setI[s] == 0) return;

                this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

                DateTime start = SS[sT[setI[s]]]; DateTime end = SE[edp[setI[s + 1] - 1]];
                LineChart.ChartAreas[0].AxisX.Interval = 0;

                double[] Vol = new double[(end - start).Days + 1];
                int k = 0;
                intfm = new double[1][]; intsm = new double[1][];
                for (DateTime dt = start; dt <= end; dt = dt.AddDays(1), k++)
                {
                    Vol[k] = getIntersectsVolume(s, dt, ref intfm[0], ref intsm[0]);
                }

                double mv = Vol[0];

                for (int i = 1; i < Vol.Length; i++)
                {
                    mv = Math.Max(Vol[i], mv);
                }

                ModelItemArrayChromosome MIAC = new ModelItemArrayChromosome(s == 0 ? SI : SI.Take(setI[s + 1] - setI[1]).Skip(setI[s] - setI[1]).ToArray(),
                    s == 0 ? Micp : Micp.Take(setI[s + 1] - setI[1]).Skip(setI[s] - setI[1]).ToArray(), SItems, s == 0 ? SI.Length : setI[s + 1] - setI[1], durations, OSDates, pr.Durations, s);

                Volume_Optimization_Function VOF = new Volume_Optimization_Function(); Population population;

                if (pr.getSelectedMethodIndex == 0)
                {
                    population = new Population(pr.getPop,
                                    MIAC,
                                VOF, (AForge.Genetic.ISelectionMethod)new AForge.Genetic.EliteSelection()
                                );
                }

                else if (pr.getSelectedMethodIndex == 1)
                {
                    population = new Population(pr.getPop,
                                    MIAC,
                                VOF, (AForge.Genetic.ISelectionMethod)new AForge.Genetic.RankSelection()
                                );
                }

                else
                {
                    population = new Population(pr.getPop,
                                    MIAC,
                                VOF, (AForge.Genetic.ISelectionMethod)new AForge.Genetic.RouletteWheelSelection()
                                );
                }

                Vol = new double[pr.getItr];
                for (k = 0; k <  pr.getItr; k++)
                {
                    // run one epoch of genetic algorithm
                    population.RunEpoch();

                    Vol[k] = -population.FitnessMax;
                }
                bestCh = (ModelItemArrayChromosome) population.BestChromosome;
                LineChart.Series[0].XValueType = ChartValueType.Int32;
                LineChart.Series[0].Points.Clear(); // clears all point in the coollection before updating with the new data

               for (int i = 0; i < k; i++) { LineChart.Series[0].Points.AddXY(i, Vol[i]); }

                DataPoint maxDataPoint = LineChart.Series[0].Points.FindMaxByValue();
                LineChart.ChartAreas[0].AxisY.Interval = maxDataPoint.YValues[0] / 20;

                LineChart.ChartAreas[0].CursorX.IsUserEnabled = false;
                LineChart.ChartAreas[0].CursorY.IsUserEnabled = false;
                LineChart.Titles[0].Text = "Minimum of The Maximum Intersected Volumes of " +
                    comboBox1.Items[s] + " per Iteration";
                LineChart.ChartAreas[0].AxisY.Title = "Volume (m3)\r\n(Original maximum = " + mv + ")";
                LineChart.ChartAreas[0].AxisX.Title = "Iteration number";
                this.button2.Enabled = true; this.button3.Enabled = true;
                displayControl.Controls.Clear(); LineChart.Controls.Clear(); LineChart.Controls.Add(this.button2);
                 LineChart.Controls.Add(this.button3); displayControl.Controls.Add(LineChart);
                this.Cursor = System.Windows.Forms.Cursors.Default;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int c = bestCh.Si == 0 ? setI[1] : setI[bestCh.Si + 1] - setI[1]; int n;

            object[][] val1 = new object[c - (setI[bestCh.Si] - setI[1])][]; object[][] val2 = new object[c - (setI[bestCh.Si] - setI[1])][];

            for (int i = bestCh.Si == 0 ? 0 : setI[bestCh.Si] - setI[1], j = 0; i < c; i++, j++)
            {
                int T; if (bestCh.Si == 0) { T = 1; while (setI[T] - (i + setI[1]) <= 0) { T++; } T--; }

                else { T = bestCh.Si - 1; } val1[j] = new object[3]; val2[j] = new object[6];

                SS[i] = OSDates[T].AddDays(bestCh.Start[j]); SE[i] = OSDates[T].AddDays(bestCh.End[j]);

                Micp[i].Direction = bestCh.DyamicProperties[j].Direction; Micp[i].Rate = bestCh.DyamicProperties[j].Rate;

                    val1[j][0] = Tasks[T]; val1[j][1] = SS[i]; val1[j][2] = SE[i];
                val2[j][0] = Micp[i].Rate; val2[j][1] = Micp[i].Direction.ToString();

                val2[j][2] = Micp[i].Protrusion.Min.ToString().Substring(1, Micp[i].Protrusion.Min.ToString().Length - 2);
                val2[j][3] = Micp[i].Protrusion.Max.ToString().Substring(1, Micp[i].Protrusion.Max.ToString().Length - 2);

                int m = SI[i].PropertyCategories.ElementAt(SI[i].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                    n = SI[i].PropertyCategories.ElementAt(SI[i].PropertyCategories.Count() - 1).Properties[5].Value.ToInt32();
                val2[j][4] = m; val2[j][5] = n;
            }

            String[] properties = { "Contained in Task", "Start", "End" };
            String[] Types = { "System.String", "System.DateTime", "System.DateTime" };
            String Category = "TimeLiner";


            addProperty(bestCh.ModelItems, Category, properties, val1, Types);

            properties = new[] { "Rate", "Direction", "Min Protrusion", "Max Protrusion", "SI", "I of I in S" };
            Types = new[] { "System.String", "System.String", "Autodesk.Navisworks.Api.Point3D",
                    "Autodesk.Navisworks.Api.Point3D", "System.int32", "System.int32"};
            Category = "Dynamics";

            addProperty(bestCh.ModelItems, Category, properties, val2, Types);

            int k;

            if (bestCh.Si == 0)
            {
                for (k = 0; k < Items.Length; k++)
                {
                    StreamWriter sw = new StreamWriter(new FileStream(currentDirectory + "/Selections/" +
       documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[k] + ".dn", FileMode.Truncate, FileAccess.Write));

                    using (sw)
                    {
                        n = setI[k + 1] - setI[1]; c = setI[k + 2] - setI[1]; 
                        String s1 = Micp[n].Protrusion.Min.ToString().Substring(1, Micp[n].Protrusion.Min.ToString().Length - 2);
                        String s2 = Micp[n].Protrusion.Max.ToString().Substring(1, Micp[n].Protrusion.Min.ToString().Length - 2);

                        sw.Write(s1 + "," + s2 + "," + Micp[n].Direction.ToString() + "," +
                            SS[n].ToShortDateString() + "," + SE[n].ToShortDateString());
                        for (n = n + 1; n < c; n++)
                        {
                            sw.Write(Environment.NewLine);
                            s1 = Micp[n].Protrusion.Min.ToString().Substring(1, Micp[n].Protrusion.Min.ToString().Length - 2);
                            s2 = Micp[n].Protrusion.Max.ToString().Substring(1, Micp[n].Protrusion.Min.ToString().Length - 2);

                            sw.Write(s1 + "," + s2 + "," + Micp[n].Direction.ToString() + "," +
                                SS[n].ToShortDateString() + "," + SE[n].ToShortDateString());
                        }
                        sw.Close();
                    }
                }
            }

            else
            {
                StreamWriter sw = new StreamWriter(new FileStream(currentDirectory + "/Selections/" +
           documentControl.Document.Models.RootItems.ElementAt(0).DisplayName + "/" + Items[bestCh.Si - 1] + ".dn", FileMode.Truncate, FileAccess.Write));

                using (sw)
                {
                    k = setI[bestCh.Si] - setI[1];
                    String s1 = Micp[k].Protrusion.Min.ToString().Substring(1, Micp[k].Protrusion.Min.ToString().Length - 2);
                    String s2 = Micp[k].Protrusion.Max.ToString().Substring(1, Micp[k].Protrusion.Min.ToString().Length - 2);

                    sw.Write(s1 + "," + s2 + "," + Micp[k].Direction.ToString() + "," +
                        SS[k].ToShortDateString() + "," + SE[k].ToShortDateString());
                    for (k = k + 1; k < c; k++)
                    {
                        sw.Write(Environment.NewLine);
                        s1 = Micp[k].Protrusion.Min.ToString().Substring(1, Micp[k].Protrusion.Min.ToString().Length - 2);
                        s2 = Micp[k].Protrusion.Max.ToString().Substring(1, Micp[k].Protrusion.Min.ToString().Length - 2);

                        sw.Write(s1 + "," + s2 + "," + Micp[k].Direction.ToString() + "," +
                            SS[k].ToShortDateString() + "," + SE[k].ToShortDateString());
                    }
                    sw.Close();
                }
            }

            if (Properties.T) { Properties.displayProperties(Properties.pcc); }

            for (k = 0; k < setI[1]; k++) { sT[k] = k; edp[k] = k; }

            Array.Sort(Clone(SE), edp, 0, setI[1]); Array.Sort(Clone(SS), sT, 0, setI[1]);
            int[] ip = new int[Items.Length]; int[] ie = new int[Items.Length];

            for (k = 0; k < setI[1]; k++)
            {
                //int i = SI[sT[k]].PropertyCategories.ElementAt(SI[sT[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();
                //int j = SI[edp[k]].PropertyCategories.ElementAt(SI[edp[k]].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                int i = 1; while (setI[i] - (sT[k] + setI[1]) <= 0) { i++; } i--;
                int j = 1; while (setI[j] - (edp[k] + setI[1]) <= 0) { j++; } j--;

                sT[setI[i] + ip[i - 1]] = sT[k]; edp[setI[j] + ie[j - 1]] = edp[k];

                ip[i - 1]++; ie[j - 1]++;
            }

            comboBox1.SelectedIndex = -1; LineChart.Series[0].Points.Clear();
            precalc = false;
            timeSlider1_Scroll(this, EventArgs.Empty);
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.Filter = "CSV (Comma delimited) (*.csv)|*.csv";
            saveFileDialog1.CreatePrompt = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.FileName = "Untitled.csv";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    int c = bestCh.ModelItems.Length;

                    sw.WriteLine("Item,Original Start Date,Original End Date,Modified Start Date,Modified End Date");

                    for (int i = 0; i < c; i++)
                    {
                        int j = bestCh.ModelItems[i].PropertyCategories.ElementAt(bestCh.ModelItems[i].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                        DateTime OSD = OSDates[j]; DateTime OED = OEDates[j];
                        DateTime SD = OSDates[j].AddDays(bestCh.Start[i]); DateTime ED = OSDates[j].AddDays(bestCh.End[i]);

                        sw.WriteLine(bestCh.ModelItems[i].PropertyCategories.ElementAt(0).Properties.ElementAt(0).Value.ToDisplayString() + "," +
                            OSD.ToShortDateString() + "," + OED.ToShortDateString() + "," + SD.ToShortDateString() + "," + ED.ToShortDateString());
                    }

                    sw.Close();
                    button3.Enabled = false;
                }
            }
        }

        private void mainForm_ResizeBegin(object sender, EventArgs e)
        {
            ys = this.ClientSize.Height;
        }

        private void mainForm_Resize(object sender, EventArgs e)
        {
            int y = this.ClientSize.Height - ys;

            viewControl.Size = new Size(viewControl.Size.Width, viewControl.Size.Height + y);
            glControl1.Size = new Size(glControl1.Width, viewControl.Size.Height + y);
            timeSlider1.Location = new Point(timeSlider1.Location.X, timeSlider1.Location.Y + y);
            button1.Location = new Point(button1.Location.X, button1.Location.Y + y);
        }
   }  
}
