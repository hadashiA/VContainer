using UnityEngine;
using NUnit.Framework;

namespace Tests
{
    public class UISystemProfilerAddMarkerWithNullObjectDoesNotCrash
    {
        [Test]
        public void AddMarkerShouldNotCrashWithNullObject()
        {
            UISystemProfilerApi.AddMarker("Test", null);
        }
    }
}
