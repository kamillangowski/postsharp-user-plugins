using Aspect.DesignByContract;
using PostSharp.Extensibility;

namespace DemoDbC
{
    public interface IAccount
    {
        [Dbc("amount>0")]
        void Debit(decimal amount);

        [Dbc("amount>0")]
        void Credit(decimal amount);

        decimal Balance { get; }
    }
}