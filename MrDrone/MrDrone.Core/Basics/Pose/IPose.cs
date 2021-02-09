using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Basics
{
    public interface IPose
    {
        bool IsAboutEqual(IPose pose);
        bool IsAboutEqual(IPose pose, float tolerance);
        bool IsAboutEqual(IPose pose, float translationTolerance, float rotationTolerance);
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        double i { get; set; }
        double j { get; set; }
        double k { get; set; }
        double w { get; set; }
        double[] values { get; }
    }
}
