using System;
using System.Collections.Generic;
using PostSharp.CodeWeaver;
using PostSharp.CodeModel;
using PostSharp.Collections;

namespace Torch.DesignByContract.Weaving.Advices
{
    class NonNullParameterAdvice : IAdvice
    {
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
            IList<ParameterDeclaration> args = new List<ParameterDeclaration>();
            foreach(ParameterDeclaration p in context.Method.Parameters)
            {
                foreach (CustomAttributeDeclaration c in p.CustomAttributes)
                {
                    object obj = c.ConstructRuntimeObject();
                    if (obj is NonNullAttribute)
                    {
                        args.Add(p);
                    }
                }
            }

            InstructionSequence nextSequence = null;
            InstructionSequence sequence = null;
            
            sequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence(sequence, NodePosition.Before, null);
            context.InstructionWriter.AttachInstructionSequence(sequence);

            foreach (ParameterDeclaration p in args)
            {
                context.InstructionWriter.EmitInstructionParameter(OpCodeNumber.Ldarg, p);
                context.InstructionWriter.EmitInstructionType(OpCodeNumber.Box, p.ParameterType);
                context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldnull);
                context.InstructionWriter.EmitInstruction(OpCodeNumber.Ceq);
                
                nextSequence = context.Method.MethodBody.CreateInstructionSequence();
                
                context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brfalse_S, nextSequence);
                context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, (LiteralString)p.Name);
                context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Newobj, context.Method.Module.FindMethod(typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) }), BindingOptions.Default));
                context.InstructionWriter.EmitInstruction(OpCodeNumber.Throw);

                context.InstructionWriter.DetachInstructionSequence();
                block.AddInstructionSequence(nextSequence, NodePosition.After, sequence);
                sequence = nextSequence;
                context.InstructionWriter.AttachInstructionSequence(sequence);
            }
                        
            context.InstructionWriter.DetachInstructionSequence();
        }

        #endregion
    }
}
