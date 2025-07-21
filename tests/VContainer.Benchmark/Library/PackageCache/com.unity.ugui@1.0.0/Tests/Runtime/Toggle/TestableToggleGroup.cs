using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;
using Object = UnityEngine.Object;

namespace ToggleTest
{
    class TestableToggleGroup : ToggleGroup
    {
        public bool ToggleListContains(Toggle toggle)
        {
            return m_Toggles.Contains(toggle);
        }
    }
}
