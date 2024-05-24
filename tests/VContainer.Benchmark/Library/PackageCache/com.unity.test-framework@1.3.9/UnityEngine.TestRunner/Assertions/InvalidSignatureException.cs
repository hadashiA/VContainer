using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.TestRunner
{
    internal class InvalidSignatureException : ResultStateException
    {
        public InvalidSignatureException(string message)
            : base(message)
        {
        }

        public override ResultState ResultState
        {
            get { return ResultState.NotRunnable; }
        }
    }
}
