using NUnit.Framework;

namespace Core.InputField
{
    public class CharacterLimitValidation : TestBehaviourBase<UnityEngine.UI.InputField>
    {
        [Test]
        public void LimitCanNotBeNegative()
        {
            const int testValue = -1;
            m_TestObject.characterLimit = testValue;
            Assert.AreNotEqual(testValue, m_TestObject.characterLimit);
        }

        [Test]
        public void TextLengthShorterThanLimit()
        {
            const string testValue = "Test";
            m_TestObject.characterLimit = 10;
            m_TestObject.text = testValue;

            Assert.AreEqual(testValue, m_TestObject.text);
        }

        [Test]
        public void TextLengthEqualToLimit()
        {
            const string testValue = "0123456789";
            m_TestObject.characterLimit = 10;
            m_TestObject.text = testValue;

            Assert.AreEqual(testValue, m_TestObject.text);
        }

        [Test]
        public void TextLengthGreaterThanLimit()
        {
            m_TestObject.characterLimit = 10;
            m_TestObject.text = "01234567891234567890";

            Assert.AreEqual(10, m_TestObject.text.Length);
            Assert.AreEqual("0123456789", m_TestObject.text);
        }
    }
}
