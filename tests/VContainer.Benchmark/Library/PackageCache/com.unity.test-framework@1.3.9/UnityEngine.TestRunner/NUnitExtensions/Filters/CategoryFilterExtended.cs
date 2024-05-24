using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;

namespace UnityEngine.TestRunner.NUnitExtensions.Filters
{
    internal class CategoryFilterExtended : CategoryFilter
    {
        public static string k_DefaultCategory = "Uncategorized";

        public CategoryFilterExtended(string name) : base(name)
        {
        }

        public override bool Match(ITest test)
        {
            var categories = test.GetAllCategoriesFromTest();

            foreach (string category in categories)
            {
                if (Match(category))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
