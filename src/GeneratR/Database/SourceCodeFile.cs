using System.Diagnostics;

namespace GeneratR.Database
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SourceCodeFile
    {
        public SourceCodeFile()
        {
        }

        public string FolderPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        private string DebuggerDisplay => $@"FileName: ""{FileName}"", FolderPath: ""{FolderPath}""";
    }
}
