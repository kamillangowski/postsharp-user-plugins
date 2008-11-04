using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Aspect.DesignByContract;
using Aspect.DesignByContract.Enums;

namespace TestApplication.UnitTest
{
	[TestFixture]
	public class UnitTests
	{
		private CountTestElement mCountTestElement = null;
		private BalanceTestElement mBalanceTestElement = null;
		private GenericTye<string> mGenericStringElement = null;
		private GenericTye<int> mGenericIntElement = null;

		public UnitTests()
		{
			SetUp();
		}

		[SetUp]
		protected void SetUp()
		{
			mCountTestElement = new CountTestElement();
			mBalanceTestElement = new BalanceTestElement();
			mGenericStringElement = new GenericTye<string>();
			mGenericIntElement = new GenericTye<int>();
		}

		[TearDown]
		public void TearDown()
		{
			mCountTestElement =  null;
			mBalanceTestElement = null;
			mGenericStringElement = null;
			mGenericIntElement = null;
		}

		[Test]
		public void Add50ToBalanceVal()
		{
			try
			{
				mBalanceTestElement.Balance = 10;
				mBalanceTestElement.AddToBalance(50);
			}
			catch(Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
		public void Set100ToCountTestElementVal()
		{
			try
			{
				mCountTestElement.Count = 100;
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
		public void NullTomInternalObjectTestElementExc()
		{
			try
			{
				mBalanceTestElement.mInternalObjectTestElement = null;
			}
			catch 
			{
				return;
			}
			Assert.Fail("Es wurde keine Exception geworfen!");
		}

		[Test]
		public void GetGenericStringValueStringVal()
		{
			try
			{
				string value = mGenericStringElement.ValueString;
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
		public void SetGenericStringValueStringVal()
		{
			if (typeof(string).Equals(typeof(string)))
			{
				int test = 0;
			}
			if (typeof(string).Equals(typeof(int)))
			{
				int test = 0;
			}
			
			try
			{
				mGenericStringElement.ValueString = "Geht auch";
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
		public void GetGenericStringValueIntVal()
		{
			try
			{
				int value = mGenericIntElement.ValueString;
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
		public void SetGenericStringValueIntExc()
		{
			try
			{
				mGenericIntElement.ValueString = 100;
			}
			catch
			{
				return;
			}
			Assert.Fail("Es wurde keine Exception geworfen");
		}

	}

	public class GenericTye<T>
	{
		private T mValueString = default(T);
		[Dbc("(T.Equals(typeof(string)))", DbcAccessType = AccessType.OnlyOnSet)]
		public T ValueString
		{
			get 
			{
				return mValueString;
			}
			set
			{
				mValueString = value;
			}
		}
	}

	public class CountTestElement
	{
		[Dbc("(IsValid([value]))", DbcAccessType = AccessType.OnlyOnSet)]
		private int mCount = int.MinValue;

		[Dbc("(([value]>0) && ((mCount!=[old]mCount) && (mCount>=0)))", DbcAccessType = AccessType.OnlyOnSet, DbcCheckTime = CheckTime.OnlyEnsure)]
		public int Count
		{
			get { return mCount; }
			set { mCount = value; }
		}

		[Dbc("(mCount>0)", DbcCheckTime =CheckTime.OnlyRequire, DbcAccessType = AccessType.OnlyOnSet)]
		public int Count2
		{
			get { return mCount; }
			set { mCount = value; }
		}

		private bool IsValid(int value)
		{
			if (value != int.MinValue)
				return true;
			return false;
		}

	
	}

	internal class BalanceTestElement
	{
		[Dbc("mPrivateIntField > 0", DbcAccessType=AccessType.OnlyOnSet )]
		private int mPrivateIntField = int.MinValue;

		[Dbc("mInternalObjectTestElement!= null")]
		internal BalanceTestElement mInternalObjectTestElement = null;

		[Dbc("mBalance>=0", DbcAccessType = AccessType.OnlyOnGet)]
		private int mBalance = 0;

		[Dbc("addBalanceValue>0", "mBalance>0")]
		public void AddToBalance(int addBalanceValue)
		{
			mBalance += addBalanceValue;
		}

		[Dbc("subtactBalanceValue>0", "(([old]Balance != subtactBalanceValue) && (Balance>0))", DbcExceptionType = typeof (ArgumentException),DbcExceptionString = "Prüfung fehlgeschlagen")]
		public void SubtractToBalance(int subtactBalanceValue)
		{
			mBalance -= subtactBalanceValue;
		}

		[Dbc("[value]>0", "mBalance>=0", "[old]mBalance!=[value]", "[result]==mBalance")]
		public int Balance
		{
			get
			{
				return mBalance;
			}
			set
			{
				mBalance = value;
			}
		}
	}

	internal class Account
	{
		private int mBalance;
		public int SubtractToBalance(int value)
		{
			// Eintrittsprüfung
			if (value < 0)
				throw new Exception("Ungültiger Wert!");
			mBalance -= value;
			// Austrittsprufüung
			if (mBalance < 0)
			{
				mBalance += value;
				throw new Exception("Ungültiger Wert!");
			}
			return mBalance;
		}
	}

	internal class Account2
	{
		[Dbc ("mBalance>=0")]
		private int mBalance;
		[Dbc("value < 0", "[old]mBalance<mBalance")]
		public int SubtractToBalance(int value)
		{
			mBalance -= value;
			return mBalance;
		}
	}

}
