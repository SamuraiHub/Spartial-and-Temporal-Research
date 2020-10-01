using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Internal.ApiImplementation;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace Spatial_and_Temporal_Research
{
    struct ObjectItem 
    {
        private bool isdynamic;
        private double rate;
        public enum direction { Left, Right, Up, Down, Front, Back };
        private direction d;
        private BoundingBox3D protrusion;
        private DateTime startDate, endDate;
       
        public ObjectItem(double rate = 0, direction d = direction.Up, DateTime startDate = new DateTime(), DateTime endDate = new DateTime(),
            BoundingBox3D protrusion = null)
        {
            this.rate = rate; this.d = d; this.protrusion = protrusion; this.startDate = startDate; this.endDate = endDate; isdynamic = false;
        }

        public bool IsDynamic
        {
            get
            {
                return isdynamic; 
            }
            set
            {
                isdynamic = value;
            }
        }

        public double Rate
        {
            get
            {
                return rate;
            }
            set
            {
                rate = value;
            }
        }

        public direction Direction
        {
            get
            {
                return d;
            }
            set
            {
                d = value;
            }
        }

        public static direction getDirection(String dir)
        {
            if (dir == "Left" || dir == "-x")
                return direction.Left;
            else if (dir == "Right" || dir == "+x")
                return direction.Right;
            else if (dir == "Up" || dir == "+y")
                return direction.Up;
            else if (dir == "Down" || dir == "-y")
                return direction.Down;
            else if (dir == "Front" || dir == "+z")
                return direction.Front;
            else if (dir == "Back" || dir == "-z")
                return direction.Back;
            else
                throw new ArgumentException("The input direction is not valid");
        }

        public static direction getDirection(int dir)
        {
            if (dir == 0)
                return direction.Left;
            else if (dir == 1)
                return direction.Right;
            else if (dir == 2)
                return direction.Down;
            else if (dir == 3)
                return direction.Up;
            else if (dir == 4)
                return direction.Back;
            else if (dir == 5)
                return direction.Front;
            else
                throw new ArgumentException("The input integer must be between 0 and 5");
        }

        public BoundingBox3D Protrusion
        {
            get
            {
                return protrusion;
            }
            set
            {
                protrusion = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return startDate;
            }

            set
            {
                startDate = value;
            }
        }

        public DateTime EndDate
        {
            get
            {
                return endDate;
            }

            set
            {
                endDate = value;
            }
        }
    }
}
