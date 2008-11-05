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
/// <createdate>05.11.2008 21:51:59</createdate>

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TestApplicationLibaryPostSharp1_5;

namespace TestApplication2PostSharp1_5.UnitTest
{
    [TestFixture]
    public class UnitTestsAbstractClass
    {
        private AbstractClassObject mAbstractClassObject = null;

        public UnitTestsAbstractClass()
		{
			SetUp();
		}

		[SetUp]
		protected void SetUp()
		{
            mAbstractClassObject = new AbstractClassObject();
		}

		[TearDown]
		public void TearDown()
		{
			mAbstractClassObject = null;
		}

		[Test]
        public void Set150ToAbstractClass()
		{
			try
			{
				mAbstractClassObject.TestMethod(150);
			}
			catch (Exception exception)
			{
				Assert.Fail("Fehler aufgetreten:" + exception.ToString());
			}
		}

		[Test]
        public void Set50ToAbstractClass()
		{
			try
			{
				mAbstractClassObject.TestMethod(50);
			}
			catch
			{
				return;
			}
			Assert.Fail("Es wurde keine Exception geworfen!");
		}

    }

    internal class AbstractClassObject : AbstractClass
    {
        public override int TestMethod(int test)
        {
            return test;
        }
    }

}
