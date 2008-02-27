using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using PostSharp.AspNet.Configuration;

namespace PostSharp.AspNet
{
    public class AssemblyPostProcessor : IAssemblyPostProcessor
    {
        private readonly PostSharpConfiguration configuration;
        private readonly string project;
        private string applicationPath;

        public AssemblyPostProcessor()
        {
            this.configuration =  WebConfigurationManager.GetSection("postsharp") as PostSharpConfiguration;
            if (this.configuration == null)
                throw new ConfigurationErrorsException("Cannot get the 'postsharp' section from configuration.");

            if (string.IsNullOrEmpty(configuration.Project))
                project = Path.Combine(configuration.Directory, "Default.psproj");
            else
                project = configuration.Project;

            applicationPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        public void PostProcessAssembly(string path)
        {
            string commandLine = Path.Combine(this.configuration.Directory, "postsharp.exe");
            if ( !File.Exists(commandLine))
            {
                throw new ConfigurationErrorsException(
                    string.Format("Cannot find the file: '{0}'.", commandLine));
            }

            // We create our directories there and take a copy of the input assembly.
            string shadowDirectory = Path.Combine(Path.GetDirectoryName(path), "before-postsharp");
            if (!Directory.Exists(shadowDirectory))
                Directory.CreateDirectory(shadowDirectory);
            string shadowAssembly = Path.Combine(shadowDirectory, Path.GetFileName(path));
            File.Copy(path, shadowAssembly, true);
            string pdbFile = Path.ChangeExtension(path, ".pdb");
            string shadowPdb = Path.Combine(shadowDirectory, Path.GetFileName(pdbFile));
            if ( File.Exists(pdbFile))
                File.Copy(pdbFile, shadowPdb, true);
            string intermediateDirectory = Path.Combine(Path.GetDirectoryName(path), "postsharp");
            if (!Directory.Exists(intermediateDirectory))
                Directory.CreateDirectory(intermediateDirectory);
            string outputAssembly = Path.Combine(intermediateDirectory, Path.GetFileName(path));
            string outputPdb = Path.Combine(intermediateDirectory, Path.GetFileName(pdbFile));

            Process process = new Process();

            process.StartInfo.FileName = commandLine;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;


            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;


          
            StringBuilder arguments = new StringBuilder();
            arguments.AppendFormat("\"/P:Output={0}\" \"/P:IntermediateDirectory={1} \"  /P:CleanIntermediate=False /P:ReferenceDirectory=. \"/P:SearchPath=",
                                   outputAssembly, intermediateDirectory);

            foreach (NameValueConfigurationElement searchPath in this.configuration.SearchPath)
            {
                arguments.Append(searchPath.Value.Replace("~", applicationPath));
                arguments.Append(",");
            }

            arguments.Append("\" ");

            foreach (NameValueConfigurationElement parameter in this.configuration.Parameters)
            {
                arguments.Append("\"/P:");
                arguments.Append(parameter.Name);
                arguments.Append('=');
                arguments.Append(parameter.Value.Replace("~", applicationPath));
                arguments.Append("\" ");
            }

            if ( this.configuration.AttachDebugger )
            {
                arguments.Append("/Attach ");
            }

            arguments.Append("/SkipAutoUpdate \"");
            arguments.Append(this.project);
            arguments.Append("\" \"");
            arguments.Append(shadowAssembly);
            arguments.Append('\"');

            process.StartInfo.Arguments = arguments.ToString();

            LogWriter logWriter;
            string logMessage;
            if (this.configuration.Trace)
            {
                string logFile = Path.Combine(Path.GetTempPath(), 
                    string.Format("postsharp-aspnet-{0:yyyy-MM-dd}.log", DateTime.Now));
                logMessage = string.Format(" See file '{0}' for details.", logFile);
                logWriter = new LogWriter(logFile);
                logWriter.Writer.WriteLine(process.StartInfo.FileName + " " + process.StartInfo.Arguments);
                process.ErrorDataReceived += logWriter.OnPostSharpDataReceived;
                process.OutputDataReceived += logWriter.OnPostSharpDataReceived;
            }
            else
            {
                logWriter = null;
                logMessage = "";
            }

          
            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            bool success = process.WaitForExit(60000);

            if ( logWriter != null )
                logWriter.Dispose();

            if ( !success )
                throw new ApplicationException("PostSharp did not finish in due time." + logMessage);

            if ( process.ExitCode != 0 )
                throw new ApplicationException("PostSharp did not complete successfully." + logMessage);

            File.Copy(outputAssembly, path, true);
            if ( File.Exists(outputPdb))
                File.Copy(outputPdb, pdbFile, true);

            
        }

        public void Dispose()
        {
            
        }
        
        class LogWriter : IDisposable
        {
            private StreamWriter writer;

            public LogWriter( string fileName )
            {
                this.writer = new StreamWriter(fileName, true, Encoding.UTF8);
            }

            public StreamWriter Writer { get { return this.writer;  } }


            public void OnPostSharpDataReceived(object sender, DataReceivedEventArgs e)
            {
                if ( HttpContext.Current != null )
                {
                     HttpContext.Current.Trace.Write("PostSharp", e.Data);
                }
                if (writer != null)
                {
                    writer.WriteLine(e.Data);
                }
            }

            public void Dispose()
            {
                if (this.writer != null)
                {
                    this.writer.Flush();
                    this.writer.Dispose();
                    this.writer = null;
                }
            }
        }
    }
}
