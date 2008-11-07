using Aspect.DesignByContract.Exceptions;
using NUnit.Framework;

namespace DemoDbC.Tests
{
    [TestFixture]
    public class TestBankAccount
    {
        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestNegativeDebit()
        {
            BankAccount account = new BankAccount();
            account.Debit(-1);
        }

        [Test]
        [ExpectedException(typeof (ContractException))]
        public void TestNegativeCredit()
        {
            BankAccount account = new BankAccount();
            account.Credit(-1);
        }

        [Test]
        public void TestGoToNegative()
        {
            BankAccount account = new BankAccount();
            account.Debit(5);
            Assert.AreEqual(account.Balance, -5);
        }
    }
}