using Knit.Utility;
using System;
using System.Collections;

namespace BSAML
{
    public struct LayoutInformation : IEquatable<LayoutInformation>
    {
        /// <summary>
        /// Gets the width of the layout object.
        /// </summary>
        /// <remarks>
        /// If this is <see langword="null"/> when it is provided to an <see cref="Element"/> to be sized,
        /// then this dimension is considered unconstrained.
        /// </remarks>
        public float? Width { get; }
        /// <summary>
        /// Gets the height of the layout object.
        /// </summary>
        /// <remarks>
        /// If this is <see langword="null"/> when it is provided to an <see cref="Element"/> to be sized,
        /// then this dimension is considered unconstrained.
        /// </remarks>
        public float? Height { get; }

        /// <summary>
        /// Gets the axis to prefer adjusting along, if any.
        /// </summary>
        public Axis PreferChangesAlong { get; }

        public LayoutInformation(float? width, float? height, Axis preferChanges = Axis.None)
        {
            Width = width;
            Height = height;
            PreferChangesAlong = preferChanges;
        }

        public override bool Equals(object obj)
            => obj is LayoutInformation lay && Equals(lay);

        public bool Equals(LayoutInformation other)
            => Width == other.Width && Height == other.Height && PreferChangesAlong == other.PreferChangesAlong;

        public override int GetHashCode()
        {
            int hashCode = -645282706;
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + PreferChangesAlong.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LayoutInformation a, LayoutInformation b)
            => a.Equals(b);
        public static bool operator !=(LayoutInformation a, LayoutInformation b)
            => !(a == b);
    }
}