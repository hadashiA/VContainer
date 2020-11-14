using System;

namespace VContainer.Internal
{
    sealed class TypedParameter : IInjectParameter
    {
        public readonly Type Type;
        public object Value { get; }

        public TypedParameter(Type type, object value)
        {
            Type = type;
            Value = value;
        }

        public bool Match(Type parameterType, string _) => parameterType == Type;
    }

    sealed class NamedParameter : IInjectParameter
    {
        public readonly string Name;
        public object Value { get; }

        public NamedParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public bool Match(Type _, string parameterName) => parameterName == Name;
    }
}
