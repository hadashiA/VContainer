using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class EditorVersionMessage : Message
    {
        public EditorVersionMessage()
        {
            type = "EditorVersion";
        }
    }
}
