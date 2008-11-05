using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TestApplicationLibaryPostSharp1_5;

namespace TestApplicationPostSharp1_5.UnitTest
{
	[TestFixture]
	public class UnitTests
	{
		private IContractInterfaceObject mIContractInterfaceObject = null;

		public UnitTests()
		{
			SetUp();
		}

		[SetUp]
		protected void SetUp()
		{
			mIContractInterfaceObject = new IContractInterfaceObject();
		}

		[TearDown]
		public void TearDown()
		{
			mIContractInterfaceObject = null;
		}

		[Test]
		public void Set150ToIContractInterface()
		{
			try
			{
				mIContractInterfaceObject.TestProperty=150;
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

//		[Test]
//		public void Set50ToIContractInterface()
//		{
//			try
//			{
//				mIContractInterfaceObject.TestProperty = 50;
//			}
//			catch
//			{
//				return;
//			}
//			Assert.Fail("Es wurde keine Exception geworfen!");
//		}

		[Test]
		public void SetTRUEToIContractInterface()
		{
			try
			{
				mIContractInterfaceObject.TestMethod(true);
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
		public void SetFALSEToIContractInterface()
		{
			try
			{
				mIContractInterfaceObject.TestMethod(false);
			}
			catch
			{
				return;
			}
			Assert.Fail("Es wurde keine Exception geworfen!");
		}
	}

	internal class IContractInterfaceObject : IContractInterface
	{
		private int mTestProperty = 101;
		public int TestProperty
		{
			get
			{
				return mTestProperty;
			}
			set
			{
				mTestProperty = value;
			}
		}

		private bool TestMethodValue = false;

		public bool TestMethod(bool test)
		{
			TestMethodValue = test;
			return TestMethodValue;
		}
	}
}
