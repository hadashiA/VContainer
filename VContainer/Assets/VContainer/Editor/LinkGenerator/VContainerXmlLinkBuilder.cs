using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using VContainer.Internal;

namespace VContainer.Editor.LinkGenerator
{
    internal sealed class VContainerXmlLinkBuilder
    {
        private readonly Dictionary<Assembly, List<InjectTypeInfo>> _map = new Dictionary<Assembly, List<InjectTypeInfo>>();
        private readonly XmlDocument _doc;

        public VContainerXmlLinkBuilder()
        {
            _doc = new XmlDocument();
        }

        public void Add(InjectTypeInfo info)
        {
            Assembly typeAssembly = info.Type.Assembly;

            if (!_map.ContainsKey(typeAssembly))
                _map.Add(typeAssembly, new List<InjectTypeInfo>());
            else
                _map[typeAssembly].Add(info);
        }

        public void WriteTo(XmlWriter writer)
        {
            var linkerNode = _doc.AppendChild(_doc.CreateElement("linker")); // <linker>

            foreach (var k in _map.OrderBy(a => a.Key.FullName)) {
                var assemblyNode = linkerNode.AppendChild(_doc.CreateElement("assembly")); // <assembly fullname="Assembly">
                var assemblyAttr = _doc.CreateAttribute("fullname");
                assemblyAttr.Value = k.Key.FullName;

                if (assemblyNode.Attributes != null) {
                    assemblyNode.Attributes.Append(assemblyAttr);

                    foreach (var t in k.Value.OrderBy(t => t.Type.FullName)) {
                        var typeNode = assemblyNode.AppendChild(_doc.CreateElement("type"));
                        var typeAttr = _doc.CreateAttribute("fullname");
                        typeAttr.Value = t.Type.FullName;
                        typeNode.Attributes?.Append(typeAttr);

                        if (t.InjectFields != null)
                            AppendField(typeNode, t.InjectFields);

                        if (t.InjectMethods != null)
                            AppendMethod(typeNode, t.InjectMethods);

                        if (t.InjectProperties != null)
                            AppendProperty(typeNode, t.InjectProperties);

                        if (t.InjectConstructor != null)
                            AppendConstructor(typeNode, t.InjectConstructor);
                    }
                }
            }
            
            _doc.Save(writer);
        }

        private void AppendField(XmlNode parentNode, IReadOnlyList<FieldInfo> fields)
        {
            // https://github.com/dotnet/linker/blob/main/docs/data-formats.md#preserve-only-selected-fields-on-a-type

            foreach (FieldInfo f in fields) {
                var fieldNode = parentNode.AppendChild(_doc.CreateElement("field"));
                var signatureAttr = _doc.CreateAttribute("signature");
                signatureAttr.Value = $"{f.FieldType.FullName} {f.Name}";
                fieldNode.Attributes?.Append(signatureAttr);
            }
        }

        private void AppendMethod(XmlNode parentNode, IReadOnlyList<InjectMethodInfo> methods)
        {
            // https://github.com/dotnet/linker/blob/main/docs/data-formats.md#preserve-only-selected-methods-on-a-type
            var sb = new StringBuilder();

            foreach (InjectMethodInfo m in methods) {
                var methodNode = parentNode.AppendChild(_doc.CreateElement("method"));
                var signatureAttr = _doc.CreateAttribute("signature");
                AggregateMethodSignature(sb, m.MethodInfo.ReturnType.FullName, m.MethodInfo.Name, m.ParameterInfos);
                signatureAttr.Value = sb.ToString();
                sb.Clear();

                methodNode.Attributes?.Append(signatureAttr);
            }
        }

        private void AppendProperty(XmlNode parentNode, IReadOnlyList<PropertyInfo> props)
        {
            // https://github.com/dotnet/linker/blob/main/docs/data-formats.md#preserve-only-selected-properties-on-type
            foreach (PropertyInfo p in props) {
                var propNode = parentNode.AppendChild(_doc.CreateElement("property"));
                var signatureAttr = _doc.CreateAttribute("signature");
                signatureAttr.Value = $"{p.PropertyType.FullName} {p.Name}";
                propNode.Attributes?.Append(signatureAttr);
            }
        }

        private void AppendConstructor(XmlNode parentNode, InjectConstructorInfo ctor)
        {
            // there is no info about .ctor preserving, but it works as usual method
            var sb = new StringBuilder();
            var methodNode = parentNode.AppendChild(_doc.CreateElement("method"));
            var signatureAttr = _doc.CreateAttribute("signature");
            AggregateMethodSignature(sb, typeof(void).FullName, ".ctor", ctor.ParameterInfos);
            signatureAttr.Value = sb.ToString();
            sb.Clear();

            methodNode.Attributes?.Append(signatureAttr);
        }

        private static void AggregateMethodSignature(StringBuilder sb,
            string returnType,
            string methodName,
            ParameterInfo[] @params)
        {
            sb.Append(returnType);
            sb.Append(' ');
            sb.Append(methodName);
            sb.Append('(');

            if (@params.Length == 0)
                sb.Append(')');
            else if (@params.Length == 1) {
                sb.Append(@params[0].ParameterType.FullName);
                sb.Append(')');
            }
            else if (@params.Length > 1) {
                for (var paramIdx = 0; paramIdx < @params.Length; paramIdx++) {
                    var paramInfo = @params[paramIdx];

                    if (paramIdx != @params.Length - 1) {
                        sb.Append(paramInfo.ParameterType.FullName);
                        sb.Append(", ");
                    }
                    else {
                        sb.Append(paramInfo.ParameterType.FullName);
                        sb.Append(')');
                    }
                }
            }
        }
    }
}