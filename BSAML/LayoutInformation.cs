using Knit.Utility;

namespace BSAML
{
    public struct LayoutInformation
    {
        /// <summary>
        /// The width of the layout object.
        /// </summary>
        /// <remarks>
        /// If this is <see langword="null"/> when it is provided to an <see cref="Element"/> to be sized,
        /// then this dimension is considered unconstrained.
        /// </remarks>
        public int? Width { get; }
        /// <summary>
        /// The height of the layout object.
        /// </summary>
        /// <remarks>
        /// If this is <see langword="null"/> when it is provided to an <see cref="Element"/> to be sized,
        /// then this dimension is considered unconstrained.
        /// </remarks>
        public int? Height { get; }

        public LayoutInformation(int? width, int? height)
        {
            Width = width;
            Height = height;
        }
    }
}