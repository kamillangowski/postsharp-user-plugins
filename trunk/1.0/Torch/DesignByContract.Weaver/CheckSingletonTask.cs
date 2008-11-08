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
    public class CheckSingletonTask : Task, IAdviceProvider
    {
        #region IAdviceProvider Members

        public void ProvideAdvices(PostSharp.CodeWeaver.Weaver codeWeaver)
        {
            // Gets the dictionary of custom attributes.
            CustomAttributeDictionaryTask customAttributeDictionary =
                CustomAttributeDictionaryTask.GetTask(this.Project);

            // Requests an enumerator of all instances of our Singleton.
            IEnumerator<ICustomAttributeInstance> customAttributeEnumerator =
                            customAttributeDictionary.GetCustomAttributesEnumerator(typeof(SingletonAttribute), true);

            ICollection<TypeDefDeclaration> singletons = new HashSet<TypeDefDeclaration>();
            // For each instance of our Singleton.
            while (customAttributeEnumerator.MoveNext())
            {
                // Gets the type to which it applies.
                TypeDefDeclaration typeDef = customAttributeEnumerator.Current.TargetElement
                                                as TypeDefDeclaration;
                
                if (typeDef != null && !singletons.Contains(typeDef))
                {
                    singletons.Add(typeDef);
                                        
                    codeWeaver.AddTypeLevelAdvice(new SingletonAccessorAdvice(typeDef), JoinPointKinds.BeforeStaticConstructor, new Singleton<TypeDefDeclaration>(typeDef));
                    codeWeaver.AddMethodLevelAdvice(new SingletonAdvice(typeDef), 
                        null, 
                        JoinPointKinds.InsteadOfNewObject,
                        new Singleton<MetadataDeclaration>(typeDef.Methods.GetOneByName(".ctor")));
                }
            }
            singletons.Clear();
            
            foreach(AssemblyRefDeclaration assembly in this.Project.Module.AssemblyRefs)
            {
                foreach (TypeRefDeclaration type in assembly.TypeRefs)
                {
                    TypeDefDeclaration def = type.GetTypeDefinition();
                    foreach(CustomAttributeDeclaration att in def.CustomAttributes)
                    {
                        if (Object.Equals(att.Constructor.DeclaringType.GetSystemType(new Type[]{},new Type[]{}), typeof(SingletonAttribute)))
                        {
                            singletons.Add(def);
                        }
                    }
                }
            }

            foreach(TypeDefDeclaration type in singletons)
            {
                codeWeaver.AddMethodLevelAdvice(new SingletonAdvice(type), 
                        null, 
                        JoinPointKinds.InsteadOfNewObject,
                        new Singleton<MetadataDeclaration>(type.Methods.GetOneByName(".ctor")));
            }
        }

        #endregion
    }
}
