using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestApplication
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void btnAspectTest_Click(object sender, EventArgs e)
		{
			AspectTest aspectTest = new AspectTest();
			aspectTest.Show();
		}
	}
}
