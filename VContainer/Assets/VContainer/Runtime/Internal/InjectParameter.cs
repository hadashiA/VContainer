using System;
using System.Reflection;

namespace VContainer.Internal
{
    public interface IInjectParameter
    {
        bool Match(ParameterInfo parameterInfo);
        object Value { get; }
    }

    public sealed class TypedParameter : IInjectParameter
    {
        public readonly Type Type;
        public object Value { get; }

        public TypedParameter(Type type, object value)
        {
            Type = type;
            Value = value;
        }

        public bool Match(ParameterInfo parameterInfo) => parameterInfo.ParameterType == Type;
    }

    public sealed class NamedParameter : IInjectParameter
    {
        public readonly string Name;
        public object Value { get; }

        public NamedParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public bool Match(ParameterInfo parameterInfo) => parameterInfo.Name == Name;

    }
}
