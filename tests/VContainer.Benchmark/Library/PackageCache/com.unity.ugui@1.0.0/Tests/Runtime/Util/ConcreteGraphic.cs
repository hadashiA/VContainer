using UnityEngine.UI;

namespace UnityEngine.UI.Tests
{
    // Make a non-abstract Graphic.
    public class ConcreteGraphic : Graphic
    {
        public override string ToString()
        {
            return string.Format("{0} '{1}'", GetType().Name, this.gameObject.name);
        }
    }
}
