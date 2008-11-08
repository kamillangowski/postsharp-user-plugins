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
/// <createdate>05.11.2008 14:31:24</createdate>

using System;
using System.Collections.Generic;
using System.Text;
using Aspect.DesignByContract;
using PostSharp.Extensibility;

namespace TestApplicationLibaryPostSharp1_5
{
	public abstract class AbstractClass
	{
        [Dbc("test>100")]
        public abstract int TestMethod(int test);
	}
}
