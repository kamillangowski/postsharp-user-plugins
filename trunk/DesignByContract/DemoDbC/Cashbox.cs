using Aspect.DesignByContract;

namespace DemoDbC
{
    public class Cashbox : IAccount
    {
        [Dbc("balance>0")] private decimal balance;

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