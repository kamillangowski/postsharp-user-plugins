/*---------------------------------------------------------------------------
*
* (c) Copyright STP Informationstechnologie AG 2005-2008. Alle Rechte vorbehalten.
* Kopieren oder andere Vervielfältigung dieses Programms, Ausnahmen nur
* zum Zweck der Erstellung einer Sicherungskopie, ist verboten ohne
* eine zuvor schriftlich eingeholte Genehmigung der Firma 
* STP Informationstechnologie AG.
*
* ---------------------------------------------------------------------------*/
/// <originalauthor>Patrick.Jahnke</originalauthor>
/// <createdate>05.11.2008 14:32:41</createdate>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TestApplicationPostSharp1_5.UnitTest;

namespace TestApplicationPostSharp1_5
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void btnNUnit_Click(object sender, EventArgs e)
		{
			UnitTests test = new UnitTests();
			test.Set150ToIContractInterface();
			test.Set50ToIContractInterface();
			test.SetFALSEToIContractInterface();
			test.SetTRUEToIContractInterface();
			test.TearDown();
			test = null;
		}
	}
}
