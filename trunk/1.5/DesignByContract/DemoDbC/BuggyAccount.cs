using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemoDbC
{
    class BuggyAccount : IAccount
    {
        public void Debit(decimal amount)
        {
            
        }

        public void Credit(decimal amount)
        {
            
        }

        public decimal Balance
        {
            get { return 0; }
        }
    }
}
