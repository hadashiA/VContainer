namespace UnityEngine.UI
{
    /// <summary>
    /// Use this interface to modify a Material that renders a Graphic. The Material is modified before the it is passed to the CanvasRenderer.
    /// </summary>
    /// <remarks>
    /// When a Graphic sets a material that is passed (in order) to any components on the GameObject that implement IMaterialModifier. This component can modify the material to be used for rendering.
    /// </remarks>
    public interface IMaterialModifier
    {
        /// <summary>
        /// Perform material modification in this function.
        /// </summary>
        /// <param name="baseMaterial">The material that is to be modified</param>
        /// <returns>The modified material.</returns>
        Material GetModifiedMaterial(Material baseMaterial);
    }
}
