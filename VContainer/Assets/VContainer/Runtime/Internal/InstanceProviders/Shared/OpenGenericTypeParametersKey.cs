using System;

namespace VContainer.Internal
{
    public class OpenGenericTypeParametersKey
    {
        public readonly Type[] TypeParameters;
        public readonly object Key;

        public OpenGenericTypeParametersKey(Type[] typeParameters, object key)
        {
            TypeParameters = typeParameters;
            Key = key;
        }

        public override bool Equals(object obj)
        {
            if (obj is OpenGenericTypeParametersKey other)
            {
                if (Key != other.Key)
                    return false;

                if (TypeParameters.Length != other.TypeParameters.Length)
                    return false;

                for (var i = 0; i < TypeParameters.Length; i++)
                {
                    if (TypeParameters[i] != other.TypeParameters[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hash = 5381;
            foreach (var typeParameter in TypeParameters)
            {
                hash = ((hash << 5) + hash) ^ typeParameter.GetHashCode();
            }
            hash = ((hash << 5) + hash) ^ (Key?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
