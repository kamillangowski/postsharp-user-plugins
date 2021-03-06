/*

Copyright (c) 2008, Michal Dabrowski

All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the Michal Dabrowski nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

using log4net;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeWeaver;
using PostSharp.Sdk.Collections;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Tasks;

namespace Log4PostSharp.Weaver
{
  public class LogTask : Task, IAdviceProvider
  {
    #region Private Fields

    /// <summary>
    /// System.Boolean type.
    /// </summary>
    private ITypeSignature boolType;

    /// <summary>
    /// System.Object type.
    /// </summary>
    private ITypeSignature objectType;

    /// <summary>
    /// System.Globalization.CultureInfo.InvariantCulture getter.
    /// </summary>
    private IMethod invariantCultureGetter;

    /// <summary>
    /// log4net.LogManager.GetLogger(System.String) method.
    /// </summary>
    private IMethod getLoggerByStringMethod;

    /// <summary>
    /// Log4PostSharpLogHelper.RegisterLogger(System.Type) method.
    /// </summary>
    private IMethod registerLoggerMethod;

    /// <summary>
    /// Log4PostSharpLogHelper.RegisterLogger(System.Type, Log4PostSharp.LoggerNamePolicy, Log4PostSharp.LoggerNamePolicy) method.
    /// </summary>
    private IMethod registerLoggerWithPolicyMethod;

    /// <summary>
    /// log4net.ILog type.
    /// </summary>
    private ITypeSignature ilogType;

    /// <summary>
    /// System.Runtime.CompilerServices.CompilerGeneratedAttribute type.
    /// </summary>
    private IType compilerGeneratedAttributeType;

    /// <summary>
    /// Collection of support items for different log levels.
    /// </summary>
    private readonly Dictionary<LogLevel, LogLevelSupportItem> levelSupportItems = new Dictionary<LogLevel, LogLevelSupportItem>();

    /// <summary>
    /// Collection of per type logging information.
    /// </summary>
    private readonly Dictionary<TypeDefDeclaration, PerTypeLoggingData> perTypeLoggingDatas = new Dictionary<TypeDefDeclaration, PerTypeLoggingData>();

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a private static readonly field.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="type">Type of the field.</param>
    /// <returns>Private static readonly field of the specified type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
    private static FieldDefDeclaration CreateField(string name, ITypeSignature type)
    {
      if (name == null)
      {
        throw new ArgumentNullException("name");
      }
      if (type == null)
      {
        throw new ArgumentNullException("type");
      }

      FieldDefDeclaration field = new FieldDefDeclaration();
      field.Attributes = FieldAttributes.InitOnly | FieldAttributes.Private | FieldAttributes.Static;
      field.Name = name;
      field.FieldType = type;
      return field;
    }

    /// <summary>
    /// Creates <see cref="LogLevelSupportItem"/> for the specified logging level.
    /// </summary>
    /// <param name="memberNamePart">"Debug", "Info", "Warn", "Error" or "Fatal" depending on the log level the item is to be created for.</param>
    /// <returns><see cref="LogLevelSupportItem"/> for the specified level.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="memberNamePart"/> is <see langword="null"/>.</exception>
    private LogLevelSupportItem CreateSupportItem(string memberNamePart)
    {
      if (memberNamePart == null)
      {
        throw new ArgumentNullException("memberNamePart");
      }

      // Target module.
      ModuleDeclaration module = this.Project.Module;

      string isLoggingEnabledGetterName = string.Format(CultureInfo.InvariantCulture, "Is{0}Enabled", memberNamePart);
      string logStringMethodName = memberNamePart;
      string logStringExceptionMethodName = memberNamePart;
      string logCultureStringArgsMethodName = memberNamePart + "Format";
      IMethod isLoggingEnabledGetter = module.FindMethod(typeof(ILog).GetProperty(isLoggingEnabledGetterName).GetGetMethod(), BindingOptions.Default);
      IMethod logStringMethod = module.FindMethod(typeof(ILog).GetMethod(logStringMethodName, new Type[] { typeof(string) }), BindingOptions.Default);
      IMethod logStringExceptionMethod = module.FindMethod(typeof(ILog).GetMethod(logStringExceptionMethodName, new Type[] { typeof(string), typeof(Exception) }), BindingOptions.Default);
      IMethod logCultureStringArgsMethod = module.FindMethod(typeof(ILog).GetMethod(logCultureStringArgsMethodName, new Type[] { typeof(IFormatProvider), typeof(string), typeof(object[]) }), BindingOptions.Default);

      return new LogLevelSupportItem(isLoggingEnabledGetter, logStringMethod, logStringExceptionMethod, logCultureStringArgsMethod);
    }

    private void AddTypeLogginData(TypeDefDeclaration wovenType)
    {
      this.perTypeLoggingDatas.Add(wovenType, new PerTypeLoggingData());

      // Logging data for the woven type.
      PerTypeLoggingData perTypeLoggingData = this.perTypeLoggingDatas[wovenType];

      // Field where ILog instance is stored.
      FieldDefDeclaration logField = CreateField("~log4PostSharp~log", this.ilogType);
      wovenType.Fields.Add(logField);
      perTypeLoggingData.Log = logField;
    }

    #endregion

    #region Internal Properties

    /// <summary>
    /// Gets the System.Object type.
    /// </summary>
    internal ITypeSignature ObjectType
    {
      get { return this.objectType; }
    }

    /// <summary>
    /// Gets the System.Globalization.CultureInfo.InvariantCulture getter.
    /// </summary>
    internal IMethod InvariantCultureGetter
    {
      get { return this.invariantCultureGetter; }
    }


    /// <summary>
    /// Gets the log4net.LogManager.GetLogger(System.String) method.
    /// </summary>
    internal IMethod GetLoggerByStringMethod
    {
      get { return this.getLoggerByStringMethod; }
    }

    /// <summary>
    /// Gets the Log4PostSharp.LogHelper.RegisterLogger(System.Type) method.
    /// </summary>
    internal IMethod RegisterLoggerMethod
    {
      get { return this.registerLoggerMethod; }
    }

    internal IMethod RegisterLoggerWithPolicyMethod
    {
      get { return this.registerLoggerWithPolicyMethod; }
    }


    /// <summary>
    /// Gets the log4net.ILog type.
    /// </summary>
    internal ITypeSignature IlogType
    {
      get { return this.ilogType; }
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Gets <see cref="LogLevelSupportItem"/> for the specified level.
    /// </summary>
    /// <param name="level">Level to get the support item for.</param>
    /// <returns>Support item for the level.</returns>
    internal LogLevelSupportItem GetSupportItem(LogLevel level)
    {
      return this.levelSupportItems[level];
    }

    internal PerTypeLoggingData GetPerTypeLoggingData(TypeDefDeclaration type)
    {
      return this.perTypeLoggingDatas[type];
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
      // Target module.
      ModuleDeclaration module = this.Project.Module;

      // Prepare types and methods. They will be used later by advices.
      this.boolType = module.FindType(typeof(bool), BindingOptions.Default);
      this.objectType = module.FindType(typeof(object), BindingOptions.Default);
      this.invariantCultureGetter = module.FindMethod(typeof(CultureInfo).GetProperty("InvariantCulture").GetGetMethod(), BindingOptions.Default);
      this.getLoggerByStringMethod = module.FindMethod(typeof(LogManager).GetMethod("GetLogger", new Type[] { typeof(string) }), BindingOptions.Default);
      this.registerLoggerMethod = module.FindMethod(typeof(LoggerHelper).GetMethod("RegisterLogger", new Type[] { typeof(Type) }), BindingOptions.Default);
      this.registerLoggerWithPolicyMethod = module.FindMethod(typeof(LoggerHelper).GetMethod("RegisterLogger", new Type[] { typeof(Type), typeof(LoggerNamePolicy), typeof(LoggerNamePolicy) }), BindingOptions.Default);

      this.ilogType = module.FindType(typeof(ILog), BindingOptions.Default);
      this.compilerGeneratedAttributeType = module.FindType(typeof(CompilerGeneratedAttribute), BindingOptions.Default).GetTypeDefinition();

      // Prepare level support items for all levels.
      this.levelSupportItems[LogLevel.Debug] = this.CreateSupportItem("Debug");
      this.levelSupportItems[LogLevel.Info] = this.CreateSupportItem("Info");
      this.levelSupportItems[LogLevel.Warn] = this.CreateSupportItem("Warn");
      this.levelSupportItems[LogLevel.Error] = this.CreateSupportItem("Error");
      this.levelSupportItems[LogLevel.Fatal] = this.CreateSupportItem("Fatal");
    }

    #endregion

    #region IAdviceProvider Members

    public void ProvideAdvices(PostSharp.Sdk.CodeWeaver.Weaver codeWeaver)
    {
      LogInitializeAdvice.InitializeLoggerPolicies(this);

      // Gets the dictionary of custom attributes.
      var customAttributeDictionaryTask = AnnotationRepositoryTask.GetTask(this.Project);

      // Requests an enumerator of all instances of the LogAttribute.
      var customAttributeEnumerator = customAttributeDictionaryTask.GetAnnotationsOfType(typeof(LogAttribute), false);

      // For each instance of the LogAttribute. 
      while (customAttributeEnumerator.MoveNext())
      {
        // Gets the method to which it applies. 
        MethodDefDeclaration methodDef = customAttributeEnumerator.Current.TargetElement as MethodDefDeclaration;
        if (methodDef != null)
        {
          // Type whose constructor is being woven.
          TypeDefDeclaration wovenType = methodDef.DeclaringType;

          // Do not weave interface.
          if ((wovenType.Attributes & TypeAttributes.Interface) != TypeAttributes.Interface)
          {
            // Constructs a custom attribute instance. 
            LogAttribute attribute = (LogAttribute)CustomAttributeHelper.ConstructRuntimeObject(customAttributeEnumerator.Current.Value);

            bool isMethodEligibleForInjection;

            if (attribute.IncludeCompilerGeneratedCode)
            {
              // Logging code can be injected even if the method is compiler generated.
              // Method processing can safely continue.
              isMethodEligibleForInjection = true;
            }
            else
            {
              // Proceed with the method only when it is not compiler generated and 
              // its declaring type is not generated.
              isMethodEligibleForInjection = !(methodDef.CustomAttributes.Contains(this.compilerGeneratedAttributeType) || methodDef.DeclaringType.CustomAttributes.Contains(this.compilerGeneratedAttributeType));
            }

            if (isMethodEligibleForInjection)
            {
              // Build an advice based on this custom attribute.
              LogAdvice advice = new LogAdvice(this, attribute);

              // Join point kinds that are used by respective logging code.
              JoinPointKinds enterKinds = (attribute.EntryLevel != LogLevel.None) ? JoinPointKinds.BeforeMethodBody : 0;
              JoinPointKinds exitKinds = (attribute.ExitLevel != LogLevel.None) ? JoinPointKinds.AfterMethodBodySuccess : 0;
              JoinPointKinds exceptionKinds = (attribute.ExceptionLevel != LogLevel.None) ? JoinPointKinds.AfterMethodBodyException : 0;
              // Sum of all required join point kinds;
              JoinPointKinds effectiveKinds = enterKinds | exitKinds | exceptionKinds;

              // Ensure there is at least one join point the logging advice applies to.
              if (effectiveKinds != 0)
              {
                if (!this.perTypeLoggingDatas.ContainsKey(wovenType))
                {

                  AddTypeLogginData(wovenType);
                  codeWeaver.AddTypeLevelAdvice(new LogInitializeAdvice(this),
                                                JoinPointKinds.BeforeStaticConstructor,
                                                new Singleton<TypeDefDeclaration>(wovenType));

                }

                codeWeaver.AddMethodLevelAdvice(advice,
                                                new Singleton<MethodDefDeclaration>(methodDef),
                                                effectiveKinds,
                                                null);
              }
            }
          }
        }
      }

      //Check for types having LoggerAttribute and have no LogAttribute.
      customAttributeEnumerator = customAttributeDictionaryTask.GetAnnotationsOfType(typeof(LoggerAttribute), false);
      while (customAttributeEnumerator.MoveNext())
      {
        // Gets the method to which it applies. 
        TypeDefDeclaration wovenType = customAttributeEnumerator.Current.TargetElement as TypeDefDeclaration;
        if (wovenType != null && !perTypeLoggingDatas.ContainsKey(wovenType))
        {
          //Weave
          AddTypeLogginData(wovenType);
          codeWeaver.AddTypeLevelAdvice(new LogInitializeAdvice(this),
                              JoinPointKinds.BeforeStaticConstructor,
                              new Singleton<TypeDefDeclaration>(wovenType));
        }
      }

      //Check for types compatible to LoggerPolicyAttribute and have no LogAttribute.
      customAttributeEnumerator = customAttributeDictionaryTask.GetAnnotationsOfType(typeof(LoggerPolicyAttribute), false);
      while (customAttributeEnumerator.MoveNext())
      {
        // Gets the method to which it applies. 
        TypeDefDeclaration wovenType = customAttributeEnumerator.Current.TargetElement as TypeDefDeclaration;
        if (wovenType != null && !wovenType.IsModuleSpecialType && !perTypeLoggingDatas.ContainsKey(wovenType))
        {
          //Weave
          AddTypeLogginData(wovenType);
          codeWeaver.AddTypeLevelAdvice(new LogInitializeAdvice(this),
                    JoinPointKinds.BeforeStaticConstructor,
                    new Singleton<TypeDefDeclaration>(wovenType));
        }
      }
    }

    #endregion

    public IEnumerable GetLevelSupportItems()
    {
      return this.levelSupportItems.Values;
    }

    /// <summary>
    /// Produces MSIL code which adds logging fields and initializes them.
    /// </summary>
    private class LogInitializeAdvice : IAdvice
    {
      /// <summary>
      /// Task that owns this advice.
      /// </summary>
      private readonly LogTask parent;

      private static IDictionary<string, IAnnotationValue> sm_loggerPolicies =
        new Dictionary<string, IAnnotationValue>();

      public static void InitializeLoggerPolicies(LogTask parent)
      {
        //Initialize LoggerPolicyMap
        var customAttributeDictionaryTask = AnnotationRepositoryTask.GetTask(parent.Project);
        var customAttributeEnumerator = customAttributeDictionaryTask.GetAnnotationsOfType(typeof(LoggerPolicyAttribute), false);
        while (customAttributeEnumerator.MoveNext())
        {
          TypeDefDeclaration typeDef = customAttributeEnumerator.Current.TargetElement as TypeDefDeclaration;
          if (typeDef != null)
          {
            sm_loggerPolicies[typeDef.Name] = customAttributeEnumerator.Current.Value;
          }
        }
      }

      #region Public Methods

      public LogInitializeAdvice(LogTask parent)
      {
        this.parent = parent;
      }

      #endregion

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
        // Type whose constructor is being woven.
        TypeDefDeclaration wovenType = context.Method.DeclaringType;

        // Logging data for the woven type.
        PerTypeLoggingData perTypeLoggingData = this.parent.perTypeLoggingDatas[wovenType];

        InstructionSequence initializeSequence = context.Method.MethodBody.CreateInstructionSequence();

        block.AddInstructionSequence(initializeSequence, NodePosition.Before, null);

        context.InstructionWriter.AttachInstructionSequence(initializeSequence);
        context.InstructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);


        IAnnotationValue loggerPolicyAtt;
        sm_loggerPolicies.TryGetValue(wovenType.Name, out loggerPolicyAtt);

        //Use the helper to get the Logger's name.
        context.WeavingHelper.GetRuntimeType(GenericHelper.GetCanonicalGenericInstance(wovenType), context.InstructionWriter);
        // Stack: type.

        if (loggerPolicyAtt != null)
        {

          //Two additional params are needed for this method.
          LoggerPolicyAttribute att = (LoggerPolicyAttribute)(new CustomAttributeDeclaration(loggerPolicyAtt)).ConstructRuntimeObject();
          context.InstructionWriter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, (int)att.LoggerNamePolicy);
          context.InstructionWriter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, (int)att.GenericArgsLoggerNamePolicy);
          context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.parent.registerLoggerWithPolicyMethod);
          // Stack: string  (logger name)
        }
        else
        {
          context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.parent.RegisterLoggerMethod);
          // Stack: string  (logger name)
        }

        context.InstructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.parent.GetLoggerByStringMethod);
        // Stack: logger.

        // Assign logger to the log variable.
        context.InstructionWriter.EmitInstructionField(OpCodeNumber.Stsfld, GenericHelper.GetCanonicalGenericInstance(perTypeLoggingData.Log));
        // Stack: .


        context.InstructionWriter.DetachInstructionSequence();
      }

      #endregion
    }
  }
}