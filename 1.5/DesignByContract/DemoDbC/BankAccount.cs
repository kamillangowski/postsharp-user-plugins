using Aspect.DesignByContract;

namespace DemoDbC
{
    internal class BankAccount : IAccount
    {
        public decimal Limit { get; set; }

        [Dbc("balance>=Limit")] private decimal balance;

        public void Debit(decimal amount)
        {
            this.balance -= amount;
        }

        public void Credit(decimal amount)
        {
            this.balance += amount;
        }

        public decimal Balance
        {
            get { return balance; }
        }
    }
}