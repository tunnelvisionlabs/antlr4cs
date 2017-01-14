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
    using RegistryKey = Microsoft.Win32.RegistryKey;
#if NET40PLUS
    using RegistryHive = Microsoft.Win32.RegistryHive;
    using RegistryView = Microsoft.Win32.RegistryView;
#else
    using Registry = Microsoft.Win32.Registry;
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

#if NET40PLUS
        private string JavaHome
        {
            get
            {
                string javaHome;
                if (TryGetJavaHome(RegistryView.Default, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;

                if (TryGetJavaHome(RegistryView.Registry64, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;

                if (TryGetJavaHome(RegistryView.Registry32, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;

                if (Directory.Exists(Environment.GetEnvironmentVariable("JAVA_HOME")))
                    return Environment.GetEnvironmentVariable("JAVA_HOME");

                throw new NotSupportedException("Could not locate a Java installation.");
            }
        }

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
#else
        private string JavaHome
        {
            get
            {
                string javaHome;
                if (TryGetJavaHome(Registry.LocalMachine, JavaVendor, JavaInstallation, out javaHome))
                    return javaHome;

                if (Directory.Exists(Environment.GetEnvironmentVariable("JAVA_HOME")))
                    return Environment.GetEnvironmentVariable("JAVA_HOME");

                throw new NotSupportedException("Could not locate a Java installation.");
            }
        }

        private static bool TryGetJavaHome(RegistryKey baseKey, string vendor, string installation, out string javaHome)
        {
            javaHome = null;

            string javaKeyName = "SOFTWARE\\" + vendor + "\\" + installation;
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
#endif

        public bool Execute()
        {
            try
            {
                string java;
                try
                {
                    if (!string.IsNullOrEmpty(JavaExecutable))
                    {
                        java = JavaExecutable;
                    }
                    else
                    {
                        string javaHome = JavaHome;
                        java = Path.Combine(Path.Combine(javaHome, "bin"), "java.exe");
                        if (!File.Exists(java))
                            java = Path.Combine(Path.Combine(javaHome, "bin"), "java");
                    }
                }
                catch (NotSupportedException)
                {
                    // Fall back to using IKVM
                    java = Path.Combine(Path.GetDirectoryName(ToolPath), "ikvm.exe");
                }

                List<string> arguments = new List<string>();
                arguments.Add("-cp");
                arguments.Add(ToolPath);
                arguments.Add("org.antlr.v4.CSharpTool");

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
                    string framework = TargetFrameworkVersion;
                    if (string.IsNullOrEmpty(framework))
                        framework = "v2.0";
                    else if (new Version(framework.Substring(1)) >= new Version(4, 5))
                        framework = "v4.5";

                    string language;
                    if (TargetLanguage.Equals("CSharp", StringComparison.OrdinalIgnoreCase))
                        language = TargetLanguage + '_' + framework.Replace('.', '_');
                    else
                        language = TargetLanguage;

                    arguments.Add("-Dlanguage=" + language);
                }

                if (!string.IsNullOrEmpty(TargetNamespace))
                {
                    arguments.Add("-package");
                    arguments.Add(TargetNamespace);
                }

                arguments.AddRange(SourceCodeFiles);

                ProcessStartInfo startInfo = new ProcessStartInfo(java, JoinArguments(arguments))
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
                process.StandardInput.Close();
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
            if (string.IsNullOrEmpty(e.Data))
                return;

            try
            {
                _buildMessages.Add(new BuildMessage(e.Data));
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
            if (string.IsNullOrEmpty(e.Data))
                return;

            try
            {
                Match match = GeneratedFileMessageFormat.Match(e.Data);
                if (!match.Success)
                {
                    _buildMessages.Add(new BuildMessage(e.Data));
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
