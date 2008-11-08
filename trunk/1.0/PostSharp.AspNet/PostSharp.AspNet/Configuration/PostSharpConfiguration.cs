using System.Configuration;
using System.Diagnostics;
using PostSharp.AspNet;

namespace PostSharp.AspNet.Configuration
{
    /// <summary>
    /// Configuration section of PostSharp ASP.NET assembly post-processor.
    /// </summary>
    /// <remarks>
    /// See <see cref="AssemblyPostProcessor"/> for a complete example
    /// of <b>web.config</b>.
    /// </remarks>
    /// <seealso cref="AssemblyPostProcessor"/>
    public sealed class PostSharpConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Determines whether PostSharp tracing is redirecting to MSBuild messages.
        /// </summary>
        /// <value>
        /// A boolean. Default is <b>true</b>.
        /// </value>
        [ConfigurationProperty("trace", DefaultValue = true)]
        public bool Trace
        {
            get { return (bool) this["trace"]; }
            set { this["trace"] = value; }
        }

        /// <summary>
        /// Gets or sets the PostSharp project to be executed.
        /// If not specified, the default project is used. 
        /// </summary>
        [ConfigurationProperty("project")]
        public string Project
        {
            get { return (string) this["project"]; }
            set { this["project"] = value; }
        }

        /// <summary>
        /// Gets collection of parameters passed to the PostSharp project.
        /// </summary>
        [ConfigurationProperty("parameters", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof (NameValueConfigurationCollection),
            AddItemName="add",
            ClearItemsName="clear",
            RemoveItemName="remove")]
        public NameValueConfigurationCollection Parameters
        {
            get { return (NameValueConfigurationCollection) this["parameters"]; }
        }

        /// <summary>
        /// Gets collection of parameters passed to the PostSharp project.
        /// </summary>
        [ConfigurationProperty("searchPath", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof (NameValueConfigurationCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public NameValueConfigurationCollection SearchPath
        {
            get { return (NameValueConfigurationCollection) this["searchPath"]; }
        }


        /// <summary>
        /// If <b>true</b>, the method <see cref="Debugger"/>.<see cref="Debugger.Launch"/>
        /// will be invoked before the execution of PostSharp, given the opportunity to
        /// attach a debugger to the building process.
        /// </summary>
        [ConfigurationProperty("attachDebugger", DefaultValue = false)]
        public bool AttachDebugger
        {
            get { return (bool) this["attachDebugger"]; }
            set { this["attachDebugger"] = value; }
        }

        /// <summary>
        /// Gets or sets the directory containing PostSharp.
        /// </summary>
        [ConfigurationProperty("directory", IsRequired = true)]
        public string Directory
        {
            get { return (string) this["directory"]; }
            set { this["directory"] = value; }
        }
    }
}