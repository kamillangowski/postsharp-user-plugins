using Aspect.DesignByContract.Exceptions;
using NUnit.Framework;

namespace DemoDbC.Tests
{
    [TestFixture]
    public class TestBuggyAccount
    {
        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestNegativeDebit()
        {
            BuggyAccount account = new BuggyAccount();
            account.Debit(-1);
        }

        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestNegativeCredit()
        {
            BuggyAccount account = new BuggyAccount();
            account.Credit(-1);
        }

        [Test]
        public void TestGoToNegative()
        {
            BuggyAccount account = new BuggyAccount();
            account.Debit(5);
            Assert.AreEqual(account.Balance, -5);
        }

        [Test]
        public void TestNormalOperations()
        {
            BuggyAccount account = new BuggyAccount();
            account.Debit(5);
            account.Debit(2);
        }

    }
}