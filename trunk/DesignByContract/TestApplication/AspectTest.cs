using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TestApplication.UnitTest;

namespace TestApplication
{
	public partial class AspectTest : Form
	{
		private UnitTests mUnitTestObject = null;
		public AspectTest()
		{
			mUnitTestObject = new UnitTests();
			InitializeComponent();
		}

		private void btnAdd50ToBalanceVal_Click(object sender, EventArgs e)
		{
			mUnitTestObject.Add50ToBalanceVal();
		}

		private void btnNullTomInternalObjectTestElementExc_Click(object sender, EventArgs e)
		{
			mUnitTestObject.NullTomInternalObjectTestElementExc();
		}

		private void btnGetGenericStringValueStringVal_Click(object sender, EventArgs e)
		{
			mUnitTestObject.GetGenericStringValueStringVal();
		}

		private void btnSetGenericStringValueStringVal_Click(object sender, EventArgs e)
		{
			mUnitTestObject.SetGenericStringValueStringVal();
		}

		private void btnGetGenericStringValueIntVal_Click(object sender, EventArgs e)
		{
			mUnitTestObject.GetGenericStringValueIntVal();
		}

		private void btnSetGenericStringValueIntExc_Click(object sender, EventArgs e)
		{
			mUnitTestObject.SetGenericStringValueIntExc();
		}

		private void btnSet100ToCountTestElementVal_Click(object sender, EventArgs e)
		{
			mUnitTestObject.Set100ToCountTestElementVal();
		}
	}
}
