using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SonicStreamer.Test
{
    [TestFixture]
    public class UnitTest
    {
        [Test]
        public void MyMethodeTest()
        {
            int a = 1;
            int b = 2;

            Assert.AreEqual(a + b, Is.EqualTo(3));
        }
    }
}
