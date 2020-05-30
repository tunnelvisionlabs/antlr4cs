// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
#if !NETSTANDARD
    using RegistryKey = Microsoft.Win32.RegistryKey;
    using RegistryHive = Microsoft.Win32.RegistryHive;
    using RegistryView = Microsoft.Win32.RegistryView;
#endif
    using StringBuilder = System.Text.StringBuilder;

    internal class AntlrClassGenerationTaskInternal
    {
        private List<string> _generatedCodeFiles = new List<string>();
        private IList<string> _sourceCodeFiles = new List<string>();
        private List<BuildMessage> _buildMessages = new List<BuildMessage>();

        public IList<string> GeneratedCodeFiles
        {
            get
            {
                return this._generatedCodeFiles;
            }
        }

        public string ToolPath
        {
            get;
            set;
        }

        public string TargetLanguage
        {
            get;
            set;
        }

        public string TargetFrameworkVersion
        {
            get;
            set;
        }

        public string OutputPath
        {
            get;
            set;
        }

        public string Encoding
        {
            get;
            set;
        }

        public string TargetNamespace
        {
            get;
            set;
        }

        public string[] LanguageSourceExtensions
        {
            get;
            set;
        }

        public bool GenerateListener
        {
            get;
            set;
        }

        public bool GenerateVisitor
        {
            get;
            set;
        }

        public bool ForceAtn
        {
            get;
            set;
        }

        public bool AbstractGrammar
        {
            get;
            set;
        }

        public string JavaVendor
        {
            get;
            set;
        }

        public string JavaInstallation
        {
            get;
            set;
        }

        public string JavaExecutable
        {
            get;
            set;
        }

        public bool UseCSharpGenerator
        {
            get;
            set;
        }

        public bool useInternalAccessModifier
        {
            get;
            set;
        }
      
        public bool ExcludeClsCompliantAttribute
        {
            get;
            set;
        }

        public bool IncludeDebuggerNonUserCodeAttribute
        {
            get;
            set;
        }

        public IList<string> SourceCodeFiles
        {
            get
            {
                return this._sourceCodeFiles;
            }
            set
            {
                this._sourceCodeFiles = value;
            }
        }

        public IList<BuildMessage> BuildMessages
        {
            get
            {
                return _buildMessages;
            }
        }

        private string JavaHome
        {
            get
            {
#if !NETSTANDARD
                string javaHome;
                if (TryGetJavaHome(RegistryView.Default, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;

                if (TryGetJavaHome(RegistryView.Registry64, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;

                if (TryGetJavaHome(RegistryView.Registry32, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;
#endif

                if (Directory.Exists(Environment.GetEnvironmentVariable("JAVA_HOME")))
                    return Environment.GetEnvironmentVariable("JAVA_HOME");

                throw new NotSupportedException("Could not locate a Java installation.");
            }
        }

#if !NETSTANDARD
        private static bool TryGetJavaHome(RegistryView registryView, string vendor, string installation, out string javaHome)
        {
            javaHome = null;

            string javaKeyName = "SOFTWARE\\" + vendor + "\\" + installation;
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                using (RegistryKey javaKey = baseKey.OpenSubKey(javaKeyName))
                {
                    if (javaKey == null)
                        return false;

                    object currentVersion = javaKey.GetValue("CurrentVersion");
                    if (currentVersion == null)
                        return false;

                    using (var homeKey = javaKey.OpenSubKey(currentVersion.ToString()))
                    {
                        if (homeKey == null || homeKey.GetValue("JavaHome") == null)
                            return false;

                        javaHome = homeKey.GetValue("JavaHome").ToString();
                        return !string.IsNullOrEmpty(javaHome);
                    }
                }
            }
        }
#endif

        public bool Execute()
        {
            try
            {
                string executable = null;
                if (!UseCSharpGenerator)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(JavaExecutable))
                        {
                            executable = JavaExecutable;
                        }
                        else
                        {
                            string javaHome = JavaHome;
                            executable = Path.Combine(Path.Combine(javaHome, "bin"), "java.exe");
                            if (!File.Exists(executable))
                                executable = Path.Combine(Path.Combine(javaHome, "bin"), "java");
                        }
                    }
                    catch (NotSupportedException)
                    {
                        // Fall back to using the new code generation tools
                        UseCSharpGenerator = true;
                    }
                }

                if (UseCSharpGenerator)
                {
#if NETSTANDARD
                    string framework = "netstandard";
                    string extension = ".dll";
#else
                    string framework = "net45";
                    string extension = ".exe";
#endif
                    executable = Path.Combine(Path.Combine(Path.GetDirectoryName(ToolPath), framework), "Antlr4" + extension);
                }

                List<string> arguments = new List<string>();

                if (!UseCSharpGenerator)
                {
                    arguments.Add("-cp");
                    arguments.Add(ToolPath);
                    arguments.Add("org.antlr.v4.CSharpTool");
                }

                arguments.Add("-o");
                arguments.Add(OutputPath);

                if (!string.IsNullOrEmpty(Encoding))
                {
                    arguments.Add("-encoding");
                    arguments.Add(Encoding);
                }

                if (GenerateListener)
                    arguments.Add("-listener");
                else
                    arguments.Add("-no-listener");

                if (GenerateVisitor)
                    arguments.Add("-visitor");
                else
                    arguments.Add("-no-visitor");

                if (ForceAtn)
                    arguments.Add("-Xforce-atn");

                if (AbstractGrammar)
                    arguments.Add("-Dabstract=true");

                if (!string.IsNullOrEmpty(TargetLanguage))
                {
                    // Since the C# target currently produces the same code for all target framework versions, we can
                    // avoid bugs with support for newer frameworks by just passing CSharp as the language and allowing
                    // the tool to use a default.
                    arguments.Add("-Dlanguage=" + TargetLanguage);
                }

                if (!string.IsNullOrEmpty(TargetNamespace))
                {
                    arguments.Add("-package");
                    arguments.Add(TargetNamespace);
                }

                arguments.AddRange(SourceCodeFiles);

                if (UseCSharpGenerator)
                {
                    if (UseInternalAccessModifier)
                        arguments.Add("-DuseInternalAccessModifier");

                    if (ExcludeClsCompliantAttribute)
                        arguments.Add("-DexcludeClsCompliantAttribute");

                    if (IncludeDebuggerNonUserCodeAttribute)
                        arguments.Add("-DincludeDebuggerNonUserCodeAttribute");
                }

#if NETSTANDARD
                if (UseCSharpGenerator)
                {
                    var outWriter = new StringWriter();
                    var errorWriter = new StringWriter();
                    try
                    {
                        var antlr = new AntlrTool(arguments.ToArray())
                        {
                            ConsoleOut = outWriter,
                            ConsoleError = errorWriter
                        };

                        antlr.ProcessGrammarsOnCommandLine();

                        return antlr.errMgr.GetNumErrors() == 0;
                    }
                    finally
                    {
                        foreach (var line in outWriter.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            HandleOutputDataReceived(line);
                        }

                        foreach (var line in errorWriter.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            HandleErrorDataReceived(line);
                        }
                    }
                }
#endif

                ProcessStartInfo startInfo = new ProcessStartInfo(executable, JoinArguments(arguments))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                this.BuildMessages.Add(new BuildMessage(TraceLevel.Info, "Executing command: \"" + startInfo.FileName + "\" " + startInfo.Arguments, "", 0, 0));

                Process process = new Process();
                process.StartInfo = startInfo;
                process.ErrorDataReceived += HandleErrorDataReceived;
                process.OutputDataReceived += HandleOutputDataReceived;
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.StandardInput.Dispose();
                process.WaitForExit();

                return process.ExitCode == 0;
                //using (LoggingTraceListener traceListener = new LoggingTraceListener(_buildMessages))
                //{
                //    SetTraceListener(traceListener);
                //    ProcessArgs(args.ToArray());
                //    process();
                //}

                //_generatedCodeFiles.AddRange(GetGeneratedFiles().Where(file => LanguageSourceExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase)));

                //int errorCount = GetNumErrors();
                //return errorCount == 0;
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException && e.InnerException != null)
                    e = e.InnerException;

                _buildMessages.Add(new BuildMessage(e.Message));
                throw;
            }
        }

        private static string JoinArguments(IEnumerable<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            StringBuilder builder = new StringBuilder();
            foreach (string argument in arguments)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                if (argument.IndexOfAny(new[] { '"', ' ' }) < 0)
                {
                    builder.Append(argument);
                    continue;
                }

                // escape a backslash appearing before a quote
                string arg = argument.Replace("\\\"", "\\\\\"");
                // escape double quotes
                arg = arg.Replace("\"", "\\\"");

                // wrap the argument in outer quotes
                builder.Append('"').Append(arg).Append('"');
            }

            return builder.ToString();
        }

        private static readonly Regex GeneratedFileMessageFormat = new Regex(@"^Generating file '(?<OUTPUT>.*?)' for grammar '(?<GRAMMAR>.*?)'$", RegexOptions.Compiled);

        private void HandleErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            HandleErrorDataReceived(e.Data);
        }

        private void HandleErrorDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            try
            {
                _buildMessages.Add(new BuildMessage(data));
            }
            catch (Exception ex)
            {
                if (Antlr4ClassGenerationTask.IsFatalException(ex))
                    throw;

                _buildMessages.Add(new BuildMessage(ex.Message));
            }
        }

        private void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            HandleOutputDataReceived(e.Data);
        }

        private void HandleOutputDataReceived(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            try
            {
                Match match = GeneratedFileMessageFormat.Match(data);
                if (!match.Success)
                {
                    _buildMessages.Add(new BuildMessage(data));
                    return;
                }

                string fileName = match.Groups["OUTPUT"].Value;
                if (LanguageSourceExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
                    GeneratedCodeFiles.Add(match.Groups["OUTPUT"].Value);
            }
            catch (Exception ex)
            {
                if (Antlr4ClassGenerationTask.IsFatalException(ex))
                    throw;

                _buildMessages.Add(new BuildMessage(ex.Message));
            }
        }
    }
}
