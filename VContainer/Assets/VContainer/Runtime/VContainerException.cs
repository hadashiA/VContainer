using System;

namespace VContainer
{
    /// <summary>
    /// Represents an error within VContainer, usually because of an unresolvable
    /// dependency or by invalid use of a method.
    /// </summary>
    public sealed class VContainerException : Exception
    {
        /// <summary>
        /// The <see cref="Type"/> that VContainer cannot process.
        /// </summary>
        public readonly Type InvalidType;

        /// <summary>
        /// Initializes a new instance of the <see cref="VContainerException"/> class.
        /// </summary>
        /// <param name="invalidType">
        /// The <see cref="Type"/> that VContainer cannot process.
        /// </param>
        /// <param name="message">
        /// Detailed information about the exception.
        /// </param>
        public VContainerException(Type invalidType, string message) : base(message)
        {
            InvalidType = invalidType;
        }
    }
}
