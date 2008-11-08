using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace Torch.DesignByContract.Weaving.Advices
{
    internal class NonEmptyParameterAdvice : IAdvice
    {
        private readonly ParameterDeclaration paramDef;

        public NonEmptyParameterAdvice(ParameterDeclaration paramDef)
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

            IMethod isNullOrEmpty = context.Method.Module.FindMethod(typeof (string).GetMethod("IsNullOrEmpty"),
                                                                     BindingOptions.Default);


            // Checks if not empty

            context.InstructionWriter.EmitInstructionParameter(OpCodeNumber.Ldarg, paramDef);
            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call, isNullOrEmpty);

            nextSequence = context.Method.MethodBody.CreateInstructionSequence();

            context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brfalse_S, nextSequence);
            context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, "Parameter is null or empty.");
            context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, this.paramDef.Name);
            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Newobj,
                                                            context.Method.Module.FindMethod(
                                                                typeof (ArgumentException).GetConstructor(new[]
                                                                                                              {
                                                                                                                  typeof
                                                                                                                      (
                                                                                                                      string
                                                                                                                      ),
                                                                                                                  typeof
                                                                                                                      (
                                                                                                                      string
                                                                                                                      )
                                                                                                              }),
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