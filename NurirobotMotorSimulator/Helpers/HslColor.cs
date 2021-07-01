using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NurirobotMotorSimulator.Helpers
{
    public struct HslColor
    {
        /// <summary>
        /// The Hue in 0..360 range.
        /// </summary>
        public double H;

        /// <summary>
        /// The Saturation in 0..1 range.
        /// </summary>
        public double S;

        /// <summary>
        /// The Lightness in 0..1 range.
        /// </summary>
        public double L;

        /// <summary>
        /// The Alpha/opacity in 0..1 range.
        /// </summary>
        public double A;
    }
}
