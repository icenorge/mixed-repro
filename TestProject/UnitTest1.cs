using MvcWebApplication;
using NUnit.Framework;
using TransientProjectB;

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
            new SomeTransientClass();
            Assert.Pass();
        }
    }
}