using Aspect.DesignByContract.Exceptions;
using NUnit.Framework;

namespace DemoDbC.Tests
{
    [TestFixture]
    public class TestCashbox
    {
        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestNegativeDebit()
        {
            Cashbox account = new Cashbox();
            account.Debit(-1);
        }

        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestNegativeCredit()
        {
            Cashbox account = new Cashbox();
            account.Credit(-1);
        }

        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestGoToNegative()
        {
            Cashbox account = new Cashbox();
            account.Debit(5);
            Assert.AreEqual(account.Balance, -5);
        }
    }
}