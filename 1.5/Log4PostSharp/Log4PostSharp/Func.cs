using System;
using System.Collections.Generic;
using System.Text;

namespace Log4PostSharp
{
    internal delegate TReturnValue Func<TParameter, TReturnValue>(TParameter parameter);
    
}
