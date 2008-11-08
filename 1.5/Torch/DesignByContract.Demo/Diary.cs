using System;

namespace DesignByContract.Demo
{
    internal class Diary : IDiary
    {
        public Contact TryFindContact(string name)
        {
            return name == "bob" ? new Contact {Name = "Bob Done"} : null;
        }

        public Contact FindContact(string name)
        {
            if (name == "bob")
                return new Contact {Name = "Bob Done"};
            else if (name == "bug")
                return null;
            else
                throw new ArgumentOutOfRangeException("name");
        }

        public void Update(Contact contact)
        {
            
        }
    }
}