/*---------------------------------------------------------------------------
*
* (c) Copyright STP Informationstechnologie AG 2005-2008. Alle Rechte vorbehalten.
* Kopieren oder andere Vervielfältigung dieses Programms, Ausnahmen nur
* zum Zweck der Erstellung einer Sicherungskopie, ist verboten ohne
* eine zuvor schriftlich eingeholte Genehmigung der Firma 
* STP Informationstechnologie AG.
*
* ---------------------------------------------------------------------------*/
/// <originalauthor>Patrick Jahnke</originalauthor>
/// <createdate>05.11.2008 23:38:02</createdate>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TestApplication2PostSharp1_5.UnitTest;

namespace TestApplication2PostSharp1_5
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnNUnit_Click(object sender, EventArgs e)
        {
            UnitTestsAbstractClass test = new UnitTestsAbstractClass();
            test.Set150ToAbstractClass();
            test.Set50ToAbstractClass();
            test.TearDown();
            test = null;

        }
    }
}
