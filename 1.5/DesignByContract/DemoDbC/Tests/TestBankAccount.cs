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
        public void TestPositiveDebit()
        {
            BankAccount account = new BankAccount();
            // If you don't take the Limit down you will get a ContractException by -> balance>=Limit of field balance
            account.Limit = -5;
            account.Debit(5);
            Assert.AreEqual(account.Balance, -5);
        }

        [Test]
        [ExpectedException(typeof(ContractException))]
        public void TestInsufficientBalance()
        {
            BankAccount account = new BankAccount();
            // If you don't take the Limit down you will get a ContractException by -> balance>=Limit of field balance
            account.Limit = -5;
            account.Debit(10);
        }

        [Test]
        public void TestNormalOperations()
        {
            BankAccount account = new BankAccount();
            account.Credit(5);
            account.Debit(2);
        }

    }
}