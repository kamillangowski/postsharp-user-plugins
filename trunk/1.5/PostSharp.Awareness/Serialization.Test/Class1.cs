using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using NUnit.Framework;
using PostSharp;
using PostSharp.Laos;

namespace Serialization.Test
{
    [TestFixture]
    public class TestDataContract
    {
        [Test]
        public void TestA()
        {
            A a = RoundtripSerialize( new A {a = 1} );
            Assert.AreEqual( 1, a.InitializeCount );
            Assert.AreEqual( 1, a.a );
            TestList( Post.Cast<A, IList<Guid>>( a ), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty );
        }

        [Test]
        public void TestB()
        {
            B<decimal> b = RoundtripSerialize( new B<decimal> {a = 1, b = 2} );
            Assert.AreEqual( 2, b.InitializeCount);
            Assert.AreEqual( 1, b.a );
            Assert.AreEqual( 2, b.b );
            TestList( Post.Cast<B<decimal>, IList<string>>( b ), "a", "b", "c" );
        }

        [Test]
        public void TestC()
        {
            C<decimal, string> c = RoundtripSerialize( new C<decimal, string> {c = 3} );
            Assert.AreEqual(1, c.InitializeCount);
            Assert.AreEqual( 3, c.c );
            Assert.AreEqual(1, c.a);
            TestList( Post.Cast<C<decimal, string>, IList<int>>( c ), 1, 2, 3 );
        }

        private static T RoundtripSerialize<T>( T o )
        {
            DataContractSerializer serializer = new DataContractSerializer( typeof(T) );
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject( stream, o );
            stream.Seek( 0, SeekOrigin.Begin );
            return (T) serializer.ReadObject( stream );
        }

        private static void TestList<T>( IList<T> list, T o1, T o2, T o3 )
        {
            list.Add( o1 );
            Assert.IsTrue( list.Contains( o1 ) );
            list.Clear();
            Assert.AreEqual( 0, list.Count );
            list.Add( o2 );
            list.Add( o3 );
            Assert.IsTrue( list[0].Equals( o2 ) );
            Assert.IsTrue( list[1].Equals( o3 ) );
        }

        [DataContract]
        [MyCompositionAspect( InterfaceType = typeof(IList<Guid>), ImplementationType = typeof(List<Guid>) )]
        private class A : IInitializeCounter
        {
            [DataMember] public int a;

            public int InitializeCount
            {
                get; set;
            }
        }

        [DataContract]
        [MyCompositionAspect( InterfaceType = typeof(IList<string>), ImplementationType = typeof(List<string>) )]
        private class B<T> : A
        {
            [DataMember] public T b;
        }

        [DataContract]
        [MyCompositionAspect( InterfaceType = typeof(IList<int>), ImplementationType = typeof(List<int>) )]
        private class C<T1, T2>  : IInitializeCounter
        {
            [DataMember] public T1 c;
            
            public int a;

            [OnDeserializing]
            private void OnDeserializing(StreamingContext streamingContext)
            {
                this.a = 1;
            }

            public int InitializeCount
            {
                get;
                set;
            }
        }
    }

    interface IInitializeCounter
    {
        int InitializeCount { get; set;  }
        
    }

    [Serializable]
    [EnableLaosAwareness( "PostSharp.Awareness.Serialization", "PostSharp.Awareness.Serialization" )]
    public class MyCompositionAspect : CompositionAspect
    {
        public Type InterfaceType;
        public Type ImplementationType;

        public override object CreateImplementationObject( InstanceBoundLaosEventArgs eventArgs )
        {
            IInitializeCounter initializeCounter = (IInitializeCounter) eventArgs.Instance;
            initializeCounter.InitializeCount = initializeCounter.InitializeCount + 1;
            
            return Activator.CreateInstance( ImplementationType );
        }

        public override Type GetPublicInterface( Type containerType )
        {
            return InterfaceType;
        }
    }
}