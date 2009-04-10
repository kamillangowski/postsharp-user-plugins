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
    /// <summary>
    /// Makes PostSharp Laos aware of serializable class by enhancing <b>OnDeserializing</b>
    /// handlers (i.e. methods annotated with the custom attribute 
    /// <see cref="OnDeserializingAttribute"/>) or by emitting a new handler if no exist before.
    /// </summary>
    public sealed class SerializationAwarenessTask : Task, ILaosAwareness
    {
        private readonly Set<TypeDefDeclaration> types = new Set<TypeDefDeclaration>();
        private readonly Set<TypeDefDeclaration> typesToEnhance = new Set<TypeDefDeclaration>();

        private LaosTask laosTask;
        private const string dataContractTypeName = "System.Runtime.Serialization.DataContractAttribute";
        private IType onDeserializingAttributeType;
        private IMethod onDeserializingAttributeConstructor;
        private IType streamingContextType;

        /// <summary>
        /// Initializes the current <see cref="SerializationAwarenessTask"/>.
        /// </summary>
        /// <param name="laosTask">The calling <see cref="LaosTask"/>.</param>
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

        void ILaosAwareness.ValidateAspects(MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers)
        {
        }

        void ILaosAwareness.BeforeImplementAspects(MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers)
        {
        }

        /// <summary>
        /// Method invoked after aspects have been implemented for each target declaration.
        /// </summary>
        /// <param name="targetDeclaration">Declaration on which targets have been applied.</param>
        /// <param name="aspectWeavers">Ordered list of aspect weavers applied to this declaration.</param>
        public void AfterImplementAspects( MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers )
        {
            // We only remember a list of the types that have been enhanced.

            TypeDefDeclaration typeDef = targetDeclaration as TypeDefDeclaration;
            if ( typeDef == null )
            {
                if ( targetDeclaration is FieldDefDeclaration )
                    typeDef = ((FieldDefDeclaration) targetDeclaration).DeclaringType;
                else if ( targetDeclaration is MethodDefDeclaration )
                    typeDef = ((MethodDefDeclaration) targetDeclaration).DeclaringType;
            }

            if (typeDef != null)
                this.types.AddIfAbsent( typeDef );
        }

        /// <summary>
        /// Determines whether a type is annotated with the <b>DataContract</b>
        /// custom attribute.
        /// </summary>
        /// <param name="typeDef">Type.</param>
        /// <returns><b>true</b> if <paramref name="typeDef"/> is annotated with the <b>DataContract</b>
        /// custom attribute, otherwise <b>false</b>.</returns>
        private static bool ContainsDataContractAttribute( TypeDefDeclaration typeDef )
        {
            foreach ( CustomAttributeDeclaration customAttribute in typeDef.CustomAttributes )
            {
                if ( ((INamedType) customAttribute.Constructor.DeclaringType).Name == dataContractTypeName )
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Method invoked after all aspects have been implemented.
        /// </summary>
        public void AfterImplementAllAspects()
        {
            foreach ( TypeDefDeclaration typeDef in types )
            {
                // Determine whether the type is serializable or annotated by [DataContract]
                if ( (typeDef.Attributes & TypeAttributes.Serializable) != 0 ||
                     ContainsDataContractAttribute( typeDef ) )
                {
                    // Get the InitializeAspect method for this type.
                    IMethod initializeMethod =
                        this.laosTask.InstanceInitializationManager.GetInitializeAspectsPrivateMethod( typeDef );

                    // If there is no InitializeAspects method, we don't need to do anything.
                    if ( initializeMethod == null )
                        return;

                    // Get the MethodDef for the InitializeAspects method.
                    MethodDefDeclaration initializeMethodDef =
                        initializeMethod.GetMethodDefinition( BindingOptions.OnlyDefinition | BindingOptions.DontThrowException );

                    // If the method is not defined in the current module, we don't need to do anything.
                    if ( initializeMethodDef == null ||
                         initializeMethodDef.Module != this.Project.Module )
                        return;

                    // Enhance this type.
                    this.EnhanceType( initializeMethodDef );
                }
            }
        }

        private void EnhanceType( MethodDefDeclaration initializeMethodDef )
        {
            TypeDefDeclaration typeDef = initializeMethodDef.DeclaringType;

            if ( !this.typesToEnhance.AddIfAbsent( typeDef ) )
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
                onDeserializingMethodDef.CustomAttributes.Add( new CustomAttributeDeclaration( onDeserializingAttributeConstructor ) );

                // Create the body of this method.
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

        /// <summary>
        /// Emits instruction that invoke <b>InitializeAspects</b>.
        /// </summary>
        /// <param name="initializeMethodDef">The <b>InitializeAspects</b> method.</param>
        /// <param name="sequence"><see cref="InstructionSequence"/> where instructions have to be emitted.</param>
        /// <param name="writer">The <see cref="InstructionWriter"/> to be used.</param>
        private static void EmitCallInitialize( MethodDefDeclaration initializeMethodDef, InstructionSequence sequence, InstructionWriter writer )
        {
            writer.AttachInstructionSequence( sequence );
            writer.EmitSymbolSequencePoint( SymbolSequencePoint.Hidden );
            writer.EmitInstruction( OpCodeNumber.Ldarg_0 );
            writer.EmitInstructionMethod( OpCodeNumber.Callvirt, GenericHelper.GetMethodCanonicalGenericInstance( initializeMethodDef ) );
            writer.DetachInstructionSequence();
        }

        /// <summary>
        /// Low-level advice that invokes <b>InitializeAspects</b> in an existing <b>OnDeserializing</b>
        /// handler.
        /// </summary>
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