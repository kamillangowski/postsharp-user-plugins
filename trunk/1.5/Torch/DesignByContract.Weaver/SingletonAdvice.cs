using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace Torch.DesignByContract.Weaving.Advices
{
    public class SingletonAdvice : IAdvice
    {
        private readonly TypeDefDeclaration m_type;

        public SingletonAdvice(TypeDefDeclaration typeDef)
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
            InstructionSequence sequence = null;

            sequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence(sequence, NodePosition.After, null);
            context.InstructionWriter.AttachInstructionSequence(sequence);

            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call,
                                                            context.Method.Module.FindMethod(
                                                                m_type.Methods.GetOneByName("get_Instance").
                                                                    GetReflectionWrapper(new Type[] {}, new Type[] {}),
                                                                BindingOptions.Default));
            context.InstructionWriter.DetachInstructionSequence();
        }

        #endregion
    }
}