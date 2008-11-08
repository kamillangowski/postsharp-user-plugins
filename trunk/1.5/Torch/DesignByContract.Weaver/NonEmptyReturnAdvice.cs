using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;

namespace Torch.DesignByContract.Weaving.Advices
{
    internal class NonEmptyReturnAdvice : IAdvice
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
            InstructionSequence sequence = context.Method.MethodBody.CreateInstructionSequence();
            block.AddInstructionSequence(sequence, NodePosition.After, null);
            InstructionWriter writer = context.InstructionWriter;
            writer.AttachInstructionSequence(sequence);

            IMethod isNullOrEmpty = context.Method.Module.FindMethod(typeof (string).GetMethod("IsNullOrEmpty"),
                                                                     BindingOptions.Default);

            context.InstructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, context.ReturnValueVariable);
            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call, isNullOrEmpty);

            InstructionSequence nextSequence = context.Method.MethodBody.CreateInstructionSequence();

            context.InstructionWriter.EmitBranchingInstruction(OpCodeNumber.Brfalse_S, nextSequence);
            context.InstructionWriter.EmitInstructionString(OpCodeNumber.Ldstr, "Return value is null or empty.");
            context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Newobj,
                                                            context.Method.Module.FindMethod(
                                                                typeof (ArgumentException).GetConstructor(new[]
                                                                                                              {
                                                                                                                  typeof
                                                                                                                      (
                                                                                                                      string
                                                                                                                      )
                                                                                                              }),
                                                                BindingOptions.Default));
            context.InstructionWriter.EmitInstruction(OpCodeNumber.Throw);

            block.AddInstructionSequence(nextSequence, NodePosition.After, sequence);

            writer.DetachInstructionSequence();
        }

        #endregion
    }
}