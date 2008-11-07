using Aspect.DesignByContract;
using PostSharp.Extensibility;

namespace DemoDbC
{
    public interface IAccount
    {
        [Dbc("amount>0", AttributeInheritance = MulticastInheritance.Strict)]
        void Debit(decimal amount);

        [Dbc("amount>0", AttributeInheritance = MulticastInheritance.Strict)]
        void Credit(decimal amount);

        decimal Balance { get; }
    }
}