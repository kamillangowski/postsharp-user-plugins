using System;
using NUnit.Framework;

namespace DesignByContract.Demo
{
    [TestFixture]
    public class Test
    {
        private readonly IDiary diary = new Diary();

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void TestEmptyParameter()
        {
            this.diary.FindContact("");
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void TestNullParameter()
        {
            this.diary.Update(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNullReturn()
        {
            Contact contact = this.diary.FindContact("bug");

            if (contact == null)
            {
                throw new Exception("We cannot be here.");
            }
        }


        [Test]
        public void TestGoodReturn()
        {
            Contact contact = this.diary.FindContact("bob");

            if (contact == null)
            {
                throw new Exception("We cannot be here.");
            }
        }
    }
}