using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace Torch.DesignByContract.Weaving.Advices
{
    public class SingletonAccessorAdvice : IAdvice
    {
        private readonly TypeDefDeclaration m_type;

        public SingletonAccessorAdvice(TypeDefDeclaration typeDef)
        {
            m_type = typeDef;
        }

        #region IAdvice Members

        public int Priority
        {
            get { return int.MinValue; }
        }

        public bool RequiresWeave(WeavingContext context)
        {
            return true;
        }

        public void Weave(WeavingContext context, InstructionBlock block)
        {
            TypeDefDeclaration typeDef = m_type;
            // Declare the static field
            FieldDefDeclaration fieldDef = new FieldDefDeclaration();
            fieldDef.Attributes = FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.NotSerialized |
                                  FieldAttributes.InitOnly;
            fieldDef.Name = "s_instance";
            fieldDef.FieldType = typeDef;

            typeDef.Fields.Add(fieldDef);

            // Declare the static accessor.
            PropertyDeclaration propDef = new PropertyDeclaration();
            propDef.Name = "Instance";

            typeDef.Properties.Add(propDef);

            MethodSemanticDeclaration sematic = new MethodSemanticDeclaration();
            sematic.Semantic = MethodSemantics.Getter;

            MethodDefDeclaration methodDef = new MethodDefDeclaration();
            methodDef.Name = "get_Instance";
            methodDef.Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static |
                                   MethodAttributes.SpecialName;
            methodDef.CallingConvention = CallingConvention.Default;

            typeDef.Methods.Add(methodDef);

            ParameterDeclaration retVal = new ParameterDeclaration();
            retVal.ParameterType = typeDef;
            retVal.Attributes = ParameterAttributes.Retval;
            methodDef.ReturnParameter = retVal;

            sematic.Method = methodDef;

            propDef.Members.Add(sematic);
            propDef.PropertyType = typeDef;

            methodDef.MethodBody = new MethodBodyDeclaration();
            methodDef.MethodBody.MaxStack = 1;
            methodDef.MethodBody.RootInstructionBlock = methodDef.MethodBody.CreateInstructionBlock();

            //throw new NotFiniteNumberException();
            InstructionSequence sequence = methodDef.MethodBody.CreateInstructionSequence();
            methodDef.MethodBody.RootInstructionBlock.AddInstructionSequence(sequence, NodePosition.After, null);
            context.InstructionWriter.AttachInstructionSequence(sequence);

            context.InstructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, typeDef.Fields.GetByName("s_instance"));
            context.InstructionWriter.EmitInstruction(OpCodeNumber.Ret);

            context.InstructionWriter.DetachInstructionSequence();

            // code for the static constructor
            sequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence(sequence, NodePosition.Before, null);
            context.InstructionWriter.AttachInstructionSequence(sequence);
            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Newobj, typeDef.Methods.GetOneByName(".ctor"));
            context.InstructionWriter.EmitInstructionField(OpCodeNumber.Stsfld, typeDef.Fields.GetByName("s_instance"));
            context.InstructionWriter.DetachInstructionSequence();

            // Change visibility to the constructor
            typeDef.Methods.GetOneByName(".ctor").Attributes = MethodAttributes.Private | MethodAttributes.HideBySig |
                                                               MethodAttributes.SpecialName |
                                                               MethodAttributes.RTSpecialName;
        }

        #endregion
    }
}