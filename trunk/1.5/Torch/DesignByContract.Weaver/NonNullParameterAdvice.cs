using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace Torch.DesignByContract.Weaving.Advices
{
    internal class NonNullParameterAdvice : IAdvice
    {
        private readonly ParameterDeclaration paramDef;

        public NonNullParameterAdvice(ParameterDeclaration paramDef)
        {
            this.paramDef = paramDef;
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
            InstructionSequence nextSequence = null;
            InstructionSequence sequence = null;

            sequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence(sequence, NodePosition.Before, null);
            context.InstructionWriter.AttachInstructionSequence(sequence);


            context.InstructionWriter.EmitInstructionParameter(OpCodeNumber.Ldarg, this.paramDef);
            context.InstructionWriter.EmitInstructionType(OpCodeNumber.Box, this.paramDef.ParameterType);
            context.InstructionWriter.EmitInstruction(OpCodeNumber.Ldnull);
            context.InstructionWriter.EmitInstruction(OpCodeNumber.Ceq);

            nextSequence = context.Method.MethodBody.CreateInstructionSequence();

            context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brfalse_S, nextSequence);
            context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, this.paramDef.Name);
            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Newobj,
                                                            context.Method.Module.FindMethod(
                                                                typeof (ArgumentNullException).GetConstructor(
                                                                    new[] {typeof (string)}),
                                                                BindingOptions.Default));
            context.InstructionWriter.EmitInstruction(OpCodeNumber.Throw);

            context.InstructionWriter.DetachInstructionSequence();
            block.AddInstructionSequence(nextSequence, NodePosition.After, sequence);
            sequence = nextSequence;
            context.InstructionWriter.AttachInstructionSequence(sequence);

            context.InstructionWriter.DetachInstructionSequence();
        }

        #endregion
    }
}