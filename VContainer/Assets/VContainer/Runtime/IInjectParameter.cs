using System;

namespace VContainer
{
    public interface IInjectParameter
    {
        bool Match(Type parameterType, string parameterName);
        object Value { get; }
    }
}