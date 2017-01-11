namespace Antlr4
{
    using Environment = System.Environment;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Environment.ExitCode = AntlrTool.Main(args);
        }
    }
}
