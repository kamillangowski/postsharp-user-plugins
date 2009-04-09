using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Laos.Weaver;

namespace PostSharp.Awareness.Serialization
{
    public sealed class SerializationAwarenessTask : Task, ILaosAwareness
    {
        private readonly Set<TypeDefDeclaration> types = new Set<TypeDefDeclaration>();
        private readonly Set<TypeDefDeclaration> typesToEnhance = new Set<TypeDefDeclaration>();

        private LaosTask laosTask;
        private const string dataContractTypeName = "System.Runtime.Serialization.DataContractAttribute";
        private IType onDeserializingAttributeType;
        private IMethod onDeserializingAttributeConstructor;
        private IType streamingContextType;

        public void Initialize( LaosTask laosTask )
        {
            this.laosTask = laosTask;
            this.onDeserializingAttributeType = (IType) this.Project.Module.GetTypeForFrameworkVariant( typeof(OnDeserializingAttribute) );
            this.onDeserializingAttributeConstructor = this.onDeserializingAttributeType.Methods.GetMethod( ".ctor",
                                                                                                            new MethodSignature( this.Project.Module,
                                                                                                                                 CallingConvention.HasThis,
                                                                                                                                 this.Project.Module.Cache.
                                                                                                                                     GetIntrinsic(
                                                                                                                                     IntrinsicType.Void ), null,
                                                                                                                                 0 ), BindingOptions.Default );
            this.streamingContextType = (IType) this.Project.Module.GetTypeForFrameworkVariant( typeof(StreamingContext) );
        }

        public void ValidateAspects( MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers )
        {
        }

        public void BeforeImplementAspects( MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers )
        {
        }

        public void AfterImplementAspects( MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers )
        {
            TypeDefDeclaration typeDef = targetDeclaration as TypeDefDeclaration;
            if ( typeDef == null )
            {
                if ( targetDeclaration is FieldDefDeclaration )
                    typeDef = ((FieldDefDeclaration) targetDeclaration).DeclaringType;
                else if ( targetDeclaration is MethodDefDeclaration )
                    typeDef = ((MethodDefDeclaration) targetDeclaration).DeclaringType;
            }

            if ( typeDef == null || !this.types.AddIfAbsent( typeDef ) )
                return;

            
        }

        private static bool ContainsDataContractAttribute( TypeDefDeclaration typeDef  )
        {
            foreach ( CustomAttributeDeclaration customAttribute in typeDef.CustomAttributes )
            {
                if ( ((INamedType) customAttribute.Constructor.DeclaringType).Name == dataContractTypeName  )
                    return true;
            }

            return false;
        }

        public void AfterImplementAllAspects()
        {
            foreach (TypeDefDeclaration typeDef in types)
            {

                // Determine whether the type is annotated by [DataContract]
                if ( (typeDef.Attributes & TypeAttributes.Serializable) != 0 ||
                     ContainsDataContractAttribute( typeDef ) )
                {
                    IMethod initializeMethod =
                        this.laosTask.InstanceInitializationManager.GetInitializeAspectsProtectedMethod( typeDef );

                    if ( initializeMethod == null )
                        return;

                    MethodDefDeclaration initializeMethodDef =
                        initializeMethod.GetMethodDefinition( BindingOptions.OnlyDefinition | BindingOptions.DontThrowException );

                    if ( initializeMethodDef == null ||
                         initializeMethodDef.Module != this.Project.Module )
                        return;

                    this.EnhanceType( initializeMethodDef );
                }

                
            }
        }

        private void EnhanceType( MethodDefDeclaration initializeMethodDef)
        {
            TypeDefDeclaration typeDef = initializeMethodDef.DeclaringType;

            if (!this.typesToEnhance.AddIfAbsent(typeDef))
                return;

            MethodDefDeclaration onDeserializingMethodDef = null;

            // Look for a method annotated with the custom attribute [OnDeserializing].
            foreach ( MethodDefDeclaration method in typeDef.Methods )
            {
                if ( method.CustomAttributes.Contains( this.onDeserializingAttributeType ) )
                {
                    onDeserializingMethodDef = method;
                    break;
                }
            }


            if ( onDeserializingMethodDef == null )
            {
                // If we did not find a method annotated with [OnDeserializing], we should define our.
                onDeserializingMethodDef = new MethodDefDeclaration
                                               {
                                                   Name = "~OnDeserializingInitializeAspects",
                                                   Attributes = MethodAttributes.Private,
                                                   CallingConvention = CallingConvention.HasThis
                                               };
                typeDef.Methods.Add( onDeserializingMethodDef );
                onDeserializingMethodDef.ReturnParameter =
                    ParameterDeclaration.CreateReturnParameter( this.Project.Module.Cache.GetIntrinsic( typeof(void) ) );
                onDeserializingMethodDef.Parameters.Add( new ParameterDeclaration( 0, "streamingContext", this.streamingContextType ) );
                this.laosTask.WeavingHelper.AddCompilerGeneratedAttribute( onDeserializingMethodDef.CustomAttributes );
                onDeserializingMethodDef.CustomAttributes.Add( new CustomAttributeDeclaration(onDeserializingAttributeConstructor) );

                onDeserializingMethodDef.MethodBody.RootInstructionBlock = onDeserializingMethodDef.MethodBody.CreateInstructionBlock();
                InstructionSequence callSequence = onDeserializingMethodDef.MethodBody.CreateInstructionSequence();
                InstructionSequence retSequence = onDeserializingMethodDef.MethodBody.CreateInstructionSequence();
                onDeserializingMethodDef.MethodBody.RootInstructionBlock.AddInstructionSequence( callSequence, NodePosition.After, null );
                onDeserializingMethodDef.MethodBody.RootInstructionBlock.AddInstructionSequence( retSequence, NodePosition.After, null );
                using ( InstructionWriter writer = new InstructionWriter() )
                {
                    EmitCallInitialize( initializeMethodDef, callSequence, writer );

                    writer.AttachInstructionSequence( retSequence );
                    writer.EmitInstruction( OpCodeNumber.Ret );
                    writer.DetachInstructionSequence();
                }
            }
            else
            {
                // Since an OnDeserializing method already exists, we have to add our code to that method.
                this.laosTask.MethodLevelAdvices.Add( new BeforeOnDeserializingMethodAdvice( onDeserializingMethodDef, initializeMethodDef ) );
            }
        }

        private static void EmitCallInitialize( MethodDefDeclaration initializeMethodDef, InstructionSequence sequence, InstructionWriter writer )
        {
            writer.AttachInstructionSequence( sequence );
            writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );
            writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, GenericHelper.GetMethodCanonicalGenericInstance( initializeMethodDef ) );
            writer.DetachInstructionSequence();
        }

        private class BeforeOnDeserializingMethodAdvice : IMethodLevelAdvice
        {
            private readonly MethodDefDeclaration initializeMethodDef;

            public BeforeOnDeserializingMethodAdvice( MethodDefDeclaration onDeserializingMethodDef, MethodDefDeclaration initializeMethodDef )
            {
                this.Method = onDeserializingMethodDef;
                this.initializeMethodDef = initializeMethodDef;
            }

            public bool RequiresWeave( WeavingContext context )
            {
                return true;
            }

            public void Weave( WeavingContext context, InstructionBlock block )
            {
                InstructionSequence sequence = context.Method.MethodBody.CreateInstructionSequence();
                block.AddInstructionSequence( sequence, NodePosition.After, null );
                EmitCallInitialize( initializeMethodDef, sequence, context.InstructionWriter );
            }

            public int Priority
            {
                get { return int.MinValue; }
            }

            public MethodDefDeclaration Method { get; private set; }

            public MetadataDeclaration Operand
            {
                get { return null; }
            }

            public JoinPointKinds JoinPointKinds
            {
                get { return JoinPointKinds.BeforeMethodBody; }
            }
        }
    }
}