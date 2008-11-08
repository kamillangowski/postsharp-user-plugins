using Torch.DesignByContract;
using JetBrains.Annotations;

namespace DesignByContract.Demo
{
    public interface IDiary
    {
        Contact TryFindContact([NonEmpty] string name);

        [return: NonNull]
        Contact FindContact([NonEmpty] string name);

        void Update([NotNull] Contact contact);
    }
}