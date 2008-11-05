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
using System.Windows.Forms;

namespace TestApplicationPostSharp1_5
{
	static class Program
	{
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
