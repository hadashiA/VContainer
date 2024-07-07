using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using VContainer.Internal;

namespace VContainer.Tests
{
    class HasNoConstructor
    {
    }

    class HasMultipleConstructor
    {
        public HasMultipleConstructor(int x)
        {
        }

        public HasMultipleConstructor(int x, int y)
        {
        }
    }

    class HasStaticConstructor
    {
        static int Foo = 400;

        [Inject]
        public HasStaticConstructor()
        {
        }
    }

    class HasInjectConstructor
    {
        [Inject]
        public HasInjectConstructor(int x, int y)
        {
        }
    }

    class HasInjectAndNoInjectConstructor
    {
        [Inject]
        public HasInjectAndNoInjectConstructor(int x)
        {
        }

        public HasInjectAndNoInjectConstructor(int x, int y)
        {
        }
    }

    #if !VCONTAINER_SOURCE_GENERATOR
    class HasMultipleInjectConstructor
    {
        [Inject]
        public HasMultipleInjectConstructor(int x)
        {
        }

        [Inject]
        public HasMultipleInjectConstructor(int x, int y)
        {
        }
    }
    #endif

    class DuplicateInjectionBaseClass
    {
        [Inject] private readonly int _someValue;
    }

    class DuplicateInjectionIntChildClass : DuplicateInjectionBaseClass
    {
        [Inject] private readonly int _someValue;
    }

    class DuplicateInjectionFloatChildClass : DuplicateInjectionBaseClass
    {
        [Inject] private readonly float _someValue;
    }

    class DuplicateInjectionStructChildClass : DuplicateInjectionBaseClass
    {
        [Inject] private readonly Vector3 _someValue;
    }

    class DuplicateInjectionGenericChildClass : DuplicateInjectionBaseClass
    {
        [Inject] private readonly List<bool> _someValue;
    }
    
    [TestFixture]
    public class TypeAnalyzerTest
    {
        [Test]
        public void AnalyzeNoConstructor()
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasNoConstructor));
            Assert.That(injectTypeInfo.InjectConstructor.ParameterInfos.Length, Is.EqualTo(0));
        }

        [Test]
        public void AnalyzeNoAttributeConstructor()
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasMultipleConstructor));
            Assert.That(injectTypeInfo.InjectConstructor.ParameterInfos.Length, Is.EqualTo(2));
        }

        [Test]
        public void AnalyzeAttributeConstructor()
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasInjectConstructor));
            Assert.That(injectTypeInfo.InjectConstructor.ConstructorInfo.GetCustomAttribute<InjectAttribute>(), Is.Not.Null);
            Assert.That(injectTypeInfo.InjectConstructor.ConstructorInfo.GetParameters().Length, Is.EqualTo(2));
        }

        [Test]
        public void AnalyzeCombinedConstructor()
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasInjectAndNoInjectConstructor));
            Assert.That(injectTypeInfo.InjectConstructor.ConstructorInfo.GetCustomAttribute<InjectAttribute>(), Is.Not.Null);
            Assert.That(injectTypeInfo.InjectConstructor.ConstructorInfo.GetParameters().Length, Is.EqualTo(1));
        }

        #if !VCONTAINER_SOURCE_GENERATOR
        [Test]
        public void AnalyzeDuplicateAttributeConstructor()
        {
            Assert.Throws<VContainerException>(() =>
            {
                TypeAnalyzer.Analyze(typeof(HasMultipleInjectConstructor));
            });
        }
        #endif

        [Test]
        public void AnalyzeStaticConstructor()
        {
            var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasStaticConstructor));
            Assert.That(injectTypeInfo.InjectConstructor.ConstructorInfo.IsStatic, Is.False);
        }
        
        [TestCase(typeof(DuplicateInjectionIntChildClass))]
        [TestCase(typeof(DuplicateInjectionFloatChildClass))]
        [TestCase(typeof(DuplicateInjectionStructChildClass))]
        [TestCase(typeof(DuplicateInjectionGenericChildClass))]
        public void ShouldThrowDuplicateInjectionException(Type type)
        {
            Assert.Throws<VContainerException>(() => { TypeAnalyzer.Analyze(type); });
        }
    }
}
