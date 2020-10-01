using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Navisworks.Api;

namespace Spatial_and_Temporal_Research
{
    class ModelItemArrayChromosome : AForge.Genetic.ChromosomeBase

    {
        /// <summary>
        /// Chromosome's length in number of elements.
        /// </summary>
        protected int length;

        /// <summary>
        /// Minimum start value of chromosome's gene (element).
        /// </summary>
        protected DateTime[] minValues;

        /// <summary>
        /// Chromosome's values.
        /// </summary>
        protected uint[] start = null;
        protected uint[] end = null;
        protected ModelItem[] Mic = null;
        protected ModelItemCollection sItems;
        protected ObjectItem[] Micp = null;
        protected int[] durations;
        protected int[] oDurations;
        protected int si;

        /// <summary>
        /// Random number generator for chromosoms generation, crossover, mutation, etc.
        /// </summary>
        protected static Random rand = new Random();

        public int Length
        {
            get { return length; }
        }

        public int Si
        {
            get { return si; }
        }

        /// <summary>
        /// Chromosome's start values.
        /// </summary>
        /// 
        /// <remarks><para>Current start values of the chromosome.</para></remarks>
        ///
        public uint[] Start
        {
            get { return start; }
        }

        /// <summary>
        /// Chromosome's end values.
        /// </summary>
        /// 
        /// <remarks><para>Current end values of the chromosome.</para></remarks>
        ///
        public uint[] End
        {
            get { return end; }
        }

        /// <summary>
        /// Chromosome's model items.
        /// </summary>
        /// 
        /// <remarks><para>Current model items of the chromosome.</para></remarks>
        ///
        public ModelItem[] ModelItems
        {
            get { return Mic; }
        }

        /// <summary>
        /// Chromosome's dynamic propreties of its model items.
        /// </summary>
        /// 
        /// <remarks><para>Current dynamic propreties of the model items of the chromosome.</para></remarks>
        ///
        public ObjectItem[] DyamicProperties
        {
            get { return Micp; }
        }

        /// <summary>
        /// Minimum possible start value of single chromosomes element - gene.
        /// </summary>
        /// 
        /// <remarks><para>Minimum possible start value, which may be represented
        /// by single chromosome's gene (array element).</para></remarks>
        /// 
        public DateTime[] MinValues
        {
            get { return minValues; }
        }

        /// <summary>
        /// Chromosome's static items.
        /// </summary>
        /// 
        /// <remarks><para>Current static items of the chromosome.</para></remarks>
        ///
        public ModelItemCollection SItems
        {
            get { return sItems; }
        }

        public ModelItemArrayChromosome(ModelItem[] mic, ObjectItem[] micp, ModelItemCollection sItems, int length, int[] oDurations, DateTime[] minValues, int[] durations, int si)
        {
            // save parameters
            this.Mic = mic;
            this.Micp = (ObjectItem[]) micp.Clone();
            this.length = length;
            this.minValues = minValues;
            this.sItems = sItems;
            this.oDurations = oDurations;
            // allocate array
            start = new uint[mic.Length];
            end = new uint[mic.Length];

            this.durations = durations; this.si = si;

            // generate random chromosome
            Generate( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortArrayChromosome"/> class.
        /// </summary>
        /// 
        /// <param name="source">Source chromosome to copy.</param>
        /// 
        /// <remarks><para>This is a copy constructor, which creates the exact copy
        /// of specified chromosome.</para></remarks>
        /// 
        protected ModelItemArrayChromosome(ModelItemArrayChromosome source)
        {
            // copy all properties
            length = source.length;
            Mic = source.Mic;
            Micp = (ObjectItem[])source.Micp.Clone();
            minValues = source.minValues;
            sItems = source.sItems;
            start = (uint[])source.start.Clone();
            end = (uint[])source.end.Clone();
            durations = source.durations;
            oDurations = source.oDurations;
            si = source.si;
            fitness  = source.fitness;
        }

        /// <summary>
        /// Get string representation of the chromosome.
        /// </summary>
        /// 
        /// <returns>Returns string representation of the chromosome.</returns>
        ///
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // append first gene
            //sb.Append((minValues.AddDays(start[0])).ToShortDateString() + ',' + (minValues.AddDays(end[0])).ToShortDateString());
            // append all other genes
            for (int i = 1; i < Mic.Length; i++)
            {
                sb.Append(new char[]{'\r', '\n'});
                //sb.Append((minValues.AddDays(start[i])).ToShortDateString() + ',' + 
                  //        (minValues.AddDays(end[i])).ToShortDateString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate random chromosome value.
        /// </summary>
        /// 
        /// <remarks><para>Regenerates chromosome's values using a random number generator.</para>
        /// </remarks>
        /// 
        public override void Generate()
        {
            for (int i = 0; i < Mic.Length; i++)
            {
                int j = Mic[i].PropertyCategories.ElementAt(Mic[i].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

                // generate next value
                start[i] = (uint)rand.Next(oDurations[j] - durations[j] + 1);
                end[i] = (uint)rand.Next((int)start[i] + durations[j], oDurations[j] + 1);
                Micp[i].Direction = ObjectItem.getDirection(rand.Next(6));

                double X = Mic[i].BoundingBox().Size.X; double Y = Mic[i].BoundingBox().Size.Y;
                double Z = Mic[i].BoundingBox().Size.Z; double r = 0.3048 / (end[i] - start[i]);
                X = X * r; Y = Y * r; Z = Z * r;

                if (Micp[i].Direction == ObjectItem.direction.Left || Micp[i].Direction == ObjectItem.direction.Right)
                {
                    Micp[i].Rate = X;
                }
                else if (Micp[i].Direction == ObjectItem.direction.Up || Micp[i].Direction == ObjectItem.direction.Down)
                {
                    Micp[i].Rate = Y;
                }
                else
                {
                    Micp[i].Rate = Z;
                }
            }
        }

        /// <summary>
        /// Clone the chromosome.
        /// </summary>
        /// 
        /// <returns>Return's clone of the chromosome.</returns>
        /// 
        /// <remarks><para>The method clones the chromosome returning the exact copy of it.</para>
        /// </remarks>
        ///
        public override AForge.Genetic.IChromosome Clone()
        {
            return new ModelItemArrayChromosome(this);
        }


        /// <summary>
        /// Creates a new random chromosome with same parameters (factory method).
        /// </summary>
        /// 
        /// <remarks><para>The method creates a new chromosome of the same type, but randomly
        /// initialized. The method is useful as factory method for those classes, which work
        /// with chromosome's interface, but not with particular chromosome type.</para></remarks>
        ///
        public override AForge.Genetic.IChromosome CreateNew()
        {
            return new ModelItemArrayChromosome(Mic, Micp, sItems, length, oDurations, minValues, durations, si);
        }


        /// <summary>
        /// Crossover operator.
        /// </summary>
        /// 
        /// <param name="pair">Pair chromosome to crossover with.</param>
        /// 
        /// <remarks><para>The method performs crossover between two chromosomes  interchanging
        /// range of genes (array elements) between these chromosomes.</para></remarks>
        ///
        public override void Crossover(AForge.Genetic.IChromosome pair)
        {
            ModelItemArrayChromosome p = (ModelItemArrayChromosome)pair;

            // check for correct pair
            if ((p != null) && (p.Mic.Length == Mic.Length))
            {
                // crossover point
                int crossOverPoint = rand.Next(Mic.Length - 1) + 1;
                // length of chromosome to be crossed
                int crossOverLength = Mic.Length - crossOverPoint;
                // temporary array
                uint[] temp1 = new uint[crossOverLength];
                uint[] temp2 = new uint[crossOverLength];
                ObjectItem[] temp3 = new ObjectItem[crossOverLength];

                // copy part of first (this) chromosome to temp
                Array.Copy(start, crossOverPoint, temp1, 0, crossOverLength);
                Array.Copy(end, crossOverPoint, temp2, 0, crossOverLength);
                Array.Copy(Micp, crossOverPoint, temp3, 0, crossOverLength);
                // copy part of second (pair) chromosome to the first
                Array.Copy(p.start, crossOverPoint, start, crossOverPoint, crossOverLength);
                Array.Copy(p.end, crossOverPoint, end, crossOverPoint, crossOverLength);
                Array.Copy(p.Micp, crossOverPoint, Micp, crossOverPoint, crossOverLength);
                // copy temp to the second
                Array.Copy(temp1, 0, p.start, crossOverPoint, crossOverLength);
                Array.Copy(temp2, 0, p.end, crossOverPoint, crossOverLength);
                Array.Copy(temp3, 0, p.Micp, crossOverPoint, crossOverLength);
            }
        }

        /// <summary>
        /// Mutation operator.
        /// </summary>
        /// 
        /// <remarks><para>The method performs chromosome's mutation, changing randomly
        /// one of its genes (array elements).</para></remarks>
        /// 
        public override void Mutate()
        {
            // get random index
            int i = rand.Next(Mic.Length);
            int j = Mic[i].PropertyCategories.ElementAt(Mic[i].PropertyCategories.Count() - 1).Properties[4].Value.ToInt32();

            // randomize the gene
            start[i] = (uint)rand.Next(oDurations[j] - durations[j] + 1);
            end[i] = (uint)rand.Next((int)start[i] + durations[j], oDurations[j] + 1);
            Micp[i].Direction = ObjectItem.getDirection(rand.Next(6));

            double X = Mic[i].BoundingBox().Size.X; double Y = Mic[i].BoundingBox().Size.Y;
            double Z = Mic[i].BoundingBox().Size.Z; double r = 0.3048 / (end[i] - start[i]);
            X = X * r; Y = Y * r; Z = Z * r;

            if (Micp[i].Direction == ObjectItem.direction.Left || Micp[i].Direction == ObjectItem.direction.Right)
            {
                Micp[i].Rate = X;
            }
            else if (Micp[i].Direction == ObjectItem.direction.Up || Micp[i].Direction == ObjectItem.direction.Down)
            {
                Micp[i].Rate = Y;
            }
            else
            {
                Micp[i].Rate = Z;
            }
        }

    }
}
