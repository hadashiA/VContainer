using NUnit.Framework;
using ContentType = UnityEngine.UI.InputField.ContentType;

namespace Core.InputField
{
    public class ContentValidation : TestBehaviourBase<UnityEngine.UI.InputField>
    {
        [Test]
        [TestCase(ContentType.Alphanumeric, "0", "0")]
        [TestCase(ContentType.Alphanumeric, "1", "1")]
        [TestCase(ContentType.Alphanumeric, "123456", "123456")]
        [TestCase(ContentType.Alphanumeric, "0123456", "0123456")]
        [TestCase(ContentType.Alphanumeric, "111110123456", "111110123456")]
        [TestCase(ContentType.Alphanumeric, "123456", "123456")]
        [TestCase(ContentType.Alphanumeric, "-1.0", "10")]
        [TestCase(ContentType.Alphanumeric, "-00.45", "0045")]
        [TestCase(ContentType.Alphanumeric, "-1111101.23456", "111110123456")]
        [TestCase(ContentType.Alphanumeric, "Test", "Test")]
        [TestCase(ContentType.Alphanumeric, "-1-", "1")]
        [TestCase(ContentType.Alphanumeric, "--1", "1")]
        [TestCase(ContentType.Alphanumeric, "123456abc", "123456abc")]
        [TestCase(ContentType.Alphanumeric, "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789", "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789")]

        [TestCase(ContentType.DecimalNumber, "0", "0")]
        [TestCase(ContentType.DecimalNumber, "1", "1")]
        [TestCase(ContentType.DecimalNumber, "123456", "123456")]
        [TestCase(ContentType.DecimalNumber, "0123456", "0123456")]
        [TestCase(ContentType.DecimalNumber, "111110123456", "111110123456")]
        //[TestCase(ContentType.DecimalNumber, "3.14", "3.14")]
        //[TestCase(ContentType.DecimalNumber, "1.23", "1.23")]
        //[TestCase(ContentType.DecimalNumber, "1.0", "1.0")]
        //[TestCase(ContentType.DecimalNumber, "00.45", "00.45")]
        //[TestCase(ContentType.DecimalNumber, "1111101.23456", "1111101.23456")]
        //[TestCase(ContentType.DecimalNumber, "-1", "-1")]
        [TestCase(ContentType.DecimalNumber, "-123456", "-123456")]
        [TestCase(ContentType.DecimalNumber, "-0123456", "-0123456")]
        [TestCase(ContentType.DecimalNumber, "-111110123456", "-111110123456")]
        //[TestCase(ContentType.DecimalNumber, "-3.14", "-3.14")]
        //[TestCase(ContentType.DecimalNumber, "-1.23", "-1.23")]
        //[TestCase(ContentType.DecimalNumber, "-1.0", "-1.0")]
        //[TestCase(ContentType.DecimalNumber, "-00.45", "-00.45")]
        //[TestCase(ContentType.DecimalNumber, "-1111101.23456", "-1111101.23456")]
        [TestCase(ContentType.DecimalNumber, "Test", "")]
        [TestCase(ContentType.DecimalNumber, "-1-", "-1")]
        //[TestCase(ContentType.DecimalNumber, "-0", "0")]
        [TestCase(ContentType.DecimalNumber, "--1", "-1")]
        [TestCase(ContentType.DecimalNumber, "123456abc", "123456")]
        //[TestCase(ContentType.DecimalNumber, "12.34.5#6abc", "12.3456")]

        [TestCase(ContentType.EmailAddress, "name@domain.com", "name@domain.com")]
        [TestCase(ContentType.EmailAddress, "name@@@domain.com", "name@domain.com")]
        [TestCase(ContentType.EmailAddress, "name@domain.co.uk", "name@domain.co.uk")]
        [TestCase(ContentType.EmailAddress, "name.other@domain-site.co.uk", "name.other@domain-site.co.uk")]
        [TestCase(ContentType.EmailAddress, "name!#$%&'*+-/=?^_`{|}~@domain.com", "name!#$%&'*+-/=?^_`{|}~@domain.com")]

        [TestCase(ContentType.IntegerNumber, "0", "0")]
        [TestCase(ContentType.IntegerNumber, "1", "1")]
        [TestCase(ContentType.IntegerNumber, "123456", "123456")]
        [TestCase(ContentType.IntegerNumber, "0123456", "0123456")]
        [TestCase(ContentType.IntegerNumber, "111110123456", "111110123456")]
        [TestCase(ContentType.IntegerNumber, "-1", "-1")]
        [TestCase(ContentType.IntegerNumber, "-123456", "-123456")]
        [TestCase(ContentType.IntegerNumber, "-0123456", "-0123456")]
        [TestCase(ContentType.IntegerNumber, "-111110123456", "-111110123456")]
        [TestCase(ContentType.IntegerNumber, "3.14", "314")]
        [TestCase(ContentType.IntegerNumber, "Test", "")]
        [TestCase(ContentType.IntegerNumber, "-1-", "-1")]
        //[TestCase(ContentType.IntegerNumber, "-0", "0")]
        //[TestCase(ContentType.IntegerNumber, "-0", "")]
        [TestCase(ContentType.IntegerNumber, "--1", "-1")]
        [TestCase(ContentType.IntegerNumber, "123456abc", "123456")]
        [TestCase(ContentType.IntegerNumber, "12.34.5#6abc", "123456")]

        [TestCase(ContentType.Name, "john smith", "John Smith")]
        [TestCase(ContentType.Name, "mary jane", "Mary Jane")]
        [TestCase(ContentType.Name, "jOHn smIth", "John Smith")]
        [TestCase(ContentType.Name, "john123 smith123", "John Smith")]
        [TestCase(ContentType.Name, "Bucky O'Hare", "Bucky O'Hare")]
        [TestCase(ContentType.Name, "bucky o'Har'e", "Bucky O'Hare")]
        [TestCase(ContentType.Name, "first second third", "First Second Third")]

        [TestCase(ContentType.Pin, "012345", "012345")]
        [TestCase(ContentType.Pin, "012345abc", "012345")]
        [TestCase(ContentType.Pin, "0a1b2c3#45", "012345")]
        [TestCase(ContentType.Pin, "-012345", "-012345")]
        [TestCase(ContentType.Pin, " 012345", "012345")]
        public void ValueIsValidatedCorrectly(ContentType type, string testValue, string expected)
        {
            m_TestObject.contentType = type;
            m_TestObject.text = testValue;
            Assert.AreEqual(expected, m_TestObject.text);
        }
    }
}
