using System;
using System.IO;
using System.Threading.Tasks;

namespace SRTMToolbox
{
    public class SRTMFile
    {
        

        public SRTMFile(string l)
        {
            this.Location = l;
            this.Type = l.Contains("SRTM1") ? SRTMFileType.SRTM1 : SRTMFileType.SRTM3;
       
            


        }

        public string Location { get; private set; }
        public SRTMFileType Type { get; private set; }

        public async Task<FileStream> OpenRead()
        {
            await SRTM.Context.Cache(this.Location);

            return File.OpenRead(SRTM.WorkingDirectory.Directory.FullName + this.Location.Replace('/', '\\').Replace(".zip", ""));


        }
    }


    public enum SRTMFileType
    {
        SRTM1,
        SRTM3
    }
}