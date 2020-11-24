using MvcWebApplication;
using NUnit.Framework;

namespace TestProject
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            new WebAppStuff();
            Assert.Pass();
        }
    }
}