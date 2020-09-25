using Knit.Utility;

namespace BSAML
{
    public struct LayoutInformation
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
    }
}