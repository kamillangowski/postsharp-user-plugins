using System;
using System.Collections.Generic;
using System.Text;
using Aspect.DesignByContract;
using Aspect.DesignByContract.Enums;
using PostSharp.Extensibility;

namespace TestApplicationLibaryPostSharp1_5
{
	public interface IContractInterface
	{
		int TestProperty { get; set; }

		[Dbc("test==true")]
		bool TestMethod(bool test);
	}
}
