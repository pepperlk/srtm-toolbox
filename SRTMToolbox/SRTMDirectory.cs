using System.IO;

namespace SRTMToolbox
{
    public class SRTMDirectory
    {
        public SRTMDirectory()
        {
            this.Directory = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
        }

        public override string ToString()
        {
            return Directory.ToString();
        }

        public DirectoryInfo Directory { get;  set; }
    }
}