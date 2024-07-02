using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VContainer.Internal;

namespace VContainer.UnitTests
{
    [TestFixture]
    public class DuplicateInjectionTest
    {
        [TestCase(typeof(IntChildClass))]
        [TestCase(typeof(FloatChildClass))]
        [TestCase(typeof(StructChildClass))]
        [TestCase(typeof(GenericChildClass))]
        public void ShouldThrowDuplicateInjectionException(Type type)
        {
            Assert.Throws<VContainerException>(() => { TypeAnalyzer.Analyze(type); });
        }

        private class BaseClass
        {
            [Inject] private readonly int _someValue;
        }

        private class IntChildClass : BaseClass
        {
            [Inject] private readonly int _someValue;
        }

        private class FloatChildClass : BaseClass
        {
            [Inject] private readonly float _someValue;
        }

        private class StructChildClass : BaseClass
        {
            [Inject] private readonly Vector3 _someValue;
        }

        private class GenericChildClass : BaseClass
        {
            [Inject] private readonly List<bool> _someValue;
        }
    }
}