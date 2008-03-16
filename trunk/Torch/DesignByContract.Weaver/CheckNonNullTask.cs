using System;
using PostSharp;
using PostSharp.Extensibility;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility.Tasks;
using System.Collections.Generic;
using PostSharp.CodeModel;
using System.Reflection;
using Torch.DesignByContract.Weaving.Advices;

namespace Torch.DesignByContract.Weaving.Tasks
{
    public class CheckNonNullTask : Task, IAdviceProvider
    {
        #region IAdviceProvider Members

        public void ProvideAdvices(Weaver codeWeaver)
        {
            // Gets the dictionary of custom attributes.
            CustomAttributeDictionaryTask customAttributeDictionary =
                CustomAttributeDictionaryTask.GetTask(this.Project);

            // Requests an enumerator of all instances of our NonNullAttribute.
            IEnumerator<ICustomAttributeInstance> customAttributeEnumerator =
                customAttributeDictionary.GetCustomAttributesEnumerator(typeof(NonNullAttribute), true);
            // Simulating a Set
            IDictionary<MethodDefDeclaration, MethodDefDeclaration> methods = new Dictionary<MethodDefDeclaration, MethodDefDeclaration>();
            // For each instance of our NonNullAttribute.
            while (customAttributeEnumerator.MoveNext())
            {
                // Gets the parameters to which it applies.
                ParameterDeclaration paramDef = customAttributeEnumerator.Current.TargetElement
                                                 as ParameterDeclaration ;

                if (paramDef != null)
                {
                    if ((paramDef.Attributes & ParameterAttributes.Retval) == ParameterAttributes.Retval)
                    {
                        codeWeaver.AddMethodLevelAdvice(new NonNullReturnAdvice(),
                                                        new Singleton<MethodDefDeclaration>(paramDef.Parent),
                                                        JoinPointKinds.AfterMethodBodySuccess,
                                                        null);
                    }
                    else
                    {
                        if (!methods.ContainsKey(paramDef.Parent))
                        {
                            codeWeaver.AddMethodLevelAdvice(new NonNullParameterAdvice(),
                                                            new Singleton<MethodDefDeclaration>(paramDef.Parent),
                                                            JoinPointKinds.BeforeMethodBody,
                                                            null);
                            methods.Add(paramDef.Parent, paramDef.Parent);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
