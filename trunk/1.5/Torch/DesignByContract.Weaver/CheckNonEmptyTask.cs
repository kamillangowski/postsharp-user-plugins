using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using Torch.DesignByContract.Weaving.Advices;

namespace Torch.DesignByContract.Weaving.Tasks
{
    public class CheckNonEmptyTask : Task, IAdviceProvider
    {
        #region IAdviceProvider Members

        public void ProvideAdvices(Weaver codeWeaver)
        {
            // Gets the dictionary of custom attributes.
            AnnotationRepositoryTask customAttributeDictionary =
                AnnotationRepositoryTask.GetTask(this.Project);

            // Requests an enumerator of all instances of our NonEmptyAttribute.
            IEnumerator<IAnnotationInstance> customAttributeEnumerator =
                customAttributeDictionary.GetAnnotationsOfType(typeof (NonEmptyAttribute), true);

            // For each instance of our NonEmptyAttribute.
            while (customAttributeEnumerator.MoveNext())
            {
                // Gets the parameters to which it applies.
                ParameterDeclaration paramDef = customAttributeEnumerator.Current.TargetElement
                                                as ParameterDeclaration;

                if (paramDef != null)
                {
                    if ((paramDef.Attributes & ParameterAttributes.Retval) == ParameterAttributes.Retval)
                    {
                        codeWeaver.AddMethodLevelAdvice(new NonEmptyReturnAdvice(),
                                                        new Singleton<MethodDefDeclaration>(paramDef.Parent),
                                                        JoinPointKinds.AfterMethodBodySuccess,
                                                        null);
                    }
                    else
                    {
                        codeWeaver.AddMethodLevelAdvice(new NonEmptyParameterAdvice(paramDef),
                                                        new Singleton<MethodDefDeclaration>(paramDef.Parent),
                                                        JoinPointKinds.BeforeMethodBody,
                                                        null);
                    }
                }
            }
        }

        #endregion
    }
}