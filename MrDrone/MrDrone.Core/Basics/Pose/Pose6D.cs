using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Basics
{
    public class Pose6D : IPose
    {
        public Pose6D()
        {
            //others are initialized with zero
            w = 1;
        }

        public Pose6D(double[] values)
        {
            X = values[0];
            Y = values[1];
            Z = values[2];
            i = values[3];
            j = values[4];
            k = values[5];
            w = values[6];
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double i { get; set; }
        public double j { get; set; }
        public double k { get; set; }
        public double w { get; set; }

        public double[] values => new[] { X, Y, Z, i, j, k, w };

        /// <summary>
        /// Returns true if the pose is about equal to the specified pose with a tolerance of 0.1 in each dimension
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public virtual bool IsAboutEqual(IPose pose)
        {
            return IsAboutEqual(pose, 0.1f);
        }

        /// <summary>
        /// Returns true if the pose is about equal to the specified pose with a specified tolerance in each dimension
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public virtual bool IsAboutEqual(IPose pose, float tolerance)
        {
            return IsAboutEqual(pose, tolerance, tolerance);
        }

        /// <summary>
        /// Returns true if the pose is about equal to the specified pose with different tolerances for translations and rotations
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public virtual bool IsAboutEqual(IPose pose, float translationTolerance, float rotationTolerance)
        {
            var myValues = values;
            var otherValues = pose.values;

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(myValues[i] - otherValues[i]) > translationTolerance) return false;
            }
            for (int i = 3; i < 7; i++)
            {
                if (Math.Abs(myValues[i] - otherValues[i]) > rotationTolerance) return false;
            }

            return true;
        }

        #region Serialization
        /// <summary>
        /// Tells the NewtonSoft Serailizer to ignore the "values : double[]" array
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializevalues() => false;

        #endregion



    }
}
