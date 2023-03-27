using System.Text;
using System.Xml;
using NUnit.Framework;
using UnityEngine;
using VContainer.Editor.LinkGenerator;
using VContainer.Internal;
using VContainer.Tests.LinkGenerator.Fixtures;

namespace VContainer.Tests.LinkGenerator
{
    public class LinkGeneratorTests
    {
        [Test]
        public void Test()
        {
            var xmlBuilder = new VContainerXmlLinkBuilder();
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(FooEmptyCtor)));
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(Foo1ParamCtor)));
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(FooManyParamCtor)));
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(FooManyCtors)));
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(FooFields)));
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(FooAutoProperty)));
            xmlBuilder.Add(TypeAnalyzer.Analyze(typeof(FooMethod)));

            var sb = new StringBuilder();
            xmlBuilder.WriteTo(XmlWriter.Create(sb));
            Debug.Log(sb.ToString());
        }
    }
}