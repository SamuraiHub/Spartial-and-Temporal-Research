using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Navisworks.Api;

namespace Spatial_and_Temporal_Research
{
    class Volume_Optimization_Function : AForge.Genetic.IFitnessFunction
    {

        #region IFitnessFunction Members

        public double Evaluate(AForge.Genetic.IChromosome chromosome)
        {
            uint[] Start = (uint[]) ((ModelItemArrayChromosome) chromosome).Start;
            uint[] End = (uint[]) ((ModelItemArrayChromosome)chromosome).End;
            ModelItem[] Mic = (ModelItem[]) ((ModelItemArrayChromosome)chromosome).ModelItems;
            ModelItemCollection SItems = ((ModelItemArrayChromosome)chromosome).SItems;
            ObjectItem[] Micp = ((ModelItemArrayChromosome)chromosome).DyamicProperties;
            int[] t = new int[Mic.Length]; int k; for (k = 0; k < t.Length; k++) { t[k] = k; }

              Array.Sort((uint[])Start.Clone(), t); uint[] EDTS = (uint[]) End.Clone(); Array.Sort(EDTS);

            Double[] Vol = new double[((ModelItemArrayChromosome)chromosome).Length + 1]; k = 0;

            for (uint dt = Start[0]; dt <= EDTS[EDTS.Length - 1]; dt++, k++)
            {
                Vol[k] = getIntersectsVolume(Mic, Micp, SItems, dt, Start, End, t);
            }

            return -getMaxVolume(Vol);
        }

        #endregion

        private double getIntersectsVolume(ModelItem[] mic, ObjectItem[] Micp, ModelItemCollection SItems, uint date, uint[] Start, uint[] End, int[] t)
        {
            int y = 0, i = 0, j = 0; int[] i1 = new int[mic.Length]; int[] i2 = new int[mic.Length];

            ModelItemCollection mc1 = new ModelItemCollection();
            ModelItemCollection mc2 = new ModelItemCollection();

            while (y < mic.Length && Start[t[y]] <= date)
            {
                if (End[t[y]] > date)
                {
                    mc2.Add(mic[t[y]]);
                    i2[j] = t[y]; j++;
                }

                else
                {
                    mc1.Add(mic[t[y]]);
                    i1[i] = t[y]; i++;
                }

                y++;
            }

            Array.Resize(ref i2, j); Array.Resize(ref i1, i);

            double v = 0;

            if (mc2.Count > 0)
            {
                v = v + getBiSectVolume(date, SItems, mc2, Micp, Start, i1, i2, j, false);

                if (mc1.Count > 0)
                {
                    v = v + getBiSectVolume(date, mc1, mc2, Micp, Start, i1, i2, j, true);
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

                    double mw = (date - Start[i2[k]]) * Micp[i2[k]].Rate;

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

                        mw = (date - Start[i2[i]]) * Micp[i2[i]].Rate;

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

                        v = v + BBP.Intersect(bb2).Volume + BBP1.Intersect(bb1).Volume + BBP.Intersect(BBP1).Volume
                            - BBP.Intersect(bb2).Intersect(BBP1).Volume - BBP1.Intersect(bb1).Intersect(BBP).Volume;
                    }
                }
            }

            return v;
        }

        private double getBiSectVolume(uint date, ModelItemCollection mc1, ModelItemCollection mc2, ObjectItem[] Micp, uint[] Start, int[] i1, int[] i2, int j, bool isD)
        {
            double v = 0;

            double mx = mc1[0].BoundingBox().Min.X * 0.3048; double my = mc1[0].BoundingBox().Min.Y * 0.3048; double mz = mc1[0].BoundingBox().Min.Z * 0.3048;
            double px = mc1[0].BoundingBox().Max.X * 0.3048; double py = mc1[0].BoundingBox().Max.Y * 0.3048; double pz = mc1[0].BoundingBox().Max.Z * 0.3048;

            Autodesk.Navisworks.Api.Point3D bn1;
            Autodesk.Navisworks.Api.Point3D bx1;

            for (int k = 0; k < mc1.Count; k++)
            {
                bn1 = mc1[k].BoundingBox().Min;
                bx1 = mc1[k].BoundingBox().Max;

                Autodesk.Navisworks.Api.Point3D Max =
                    new Autodesk.Navisworks.Api.Point3D((bx1.X * 0.3048), (bx1.Y * 0.3048), (bx1.Z * 0.3048));

                Autodesk.Navisworks.Api.Point3D Min =
                    new Autodesk.Navisworks.Api.Point3D((bn1.X * 0.3048), (bn1.Y * 0.3048), (bn1.Z * 0.3048));

                if (isD)
                {
                    Max = Max.Add(Micp[i1[k]].Protrusion.Max.ToVector3D());
                    Min = Min.Subtract(Micp[i1[k]].Protrusion.Min.ToVector3D());
                }

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
                    }
                }

                List<BoundingBox3D> wk = new List<BoundingBox3D>(mc2.Count);

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
                        double mw = (date - Start[i2[i]]) * Micp[i2[i]].Rate;

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

                for (i = 0; i < im.Count; i++)
                {
                    for (int n = 0; n < wk.Count; n++)
                    {
                        v = v + wk[n].Intersect(im[i]).Volume;
                    }
                }
            }

            return v;
        }

        private double getMaxVolume(double[] vol)
        {
            double mv = vol[0];

            for (int i = 1; i < vol.Length; i++)
            {
                mv = Math.Max(vol[i], mv);
            }

            return mv;
        }
    }
}
