using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTMToolbox
{
    public static class SRTM
    {
        private static SRTMDirectory _workingDir;

        public static SRTMDirectory WorkingDirectory
        {
            get
            {
                if (_workingDir == null)
                {
                    _workingDir = new SRTMDirectory();
                }
                return _workingDir;
            }
            set { _workingDir = value; }
        }

        private static SRTMEndpointCollection _endpoints;

        public static SRTMEndpointCollection Endpoints
        {
            get
            {
                if (_endpoints == null)
                {
                    _endpoints = new SRTMEndpointCollection();
                    _endpoints.Add("NASA 2.1", "https://dds.cr.usgs.gov/srtm/version2_1/", SRTMEndpointType.Web);
                }

                return _endpoints;
            }
            set { _endpoints = value; }
        }


        private static SRTMContext _context;

        public static SRTMContext Context
        {
            get
            {
                if (_context == null)
                {
                    _context = new SRTMContext();
                }

                return _context;
            }
            set { _context = value; }
        }

        public static async Task<double> GetElevation(double lat, double lng)
        {
            var latangle = GeoAngle.FromDouble(lat);
            var rowl = (int)Math.Round(((latangle.Minutes * 60) + latangle.AllSeconds) / 3, 0);

            var lngangle = GeoAngle.FromDouble(lng);
            var coll = (int)Math.Round(((lngangle.Minutes * 60) + lngangle.AllSeconds) / 3,0);


            var file = SRTM.Context.GetSRTMFileForLatLng(lat, lng);


            var srtmFile = await SRTM.Context.FindSRTMFile(file);

            var stream = await srtmFile.OpenRead();

            var colrows = srtmFile.Type == SRTMFileType.SRTM1 ? 3601 : 1201;


            var numBytes = 2;






            var i = colrows - rowl;
            var j = coll;
            var pos = ((colrows * (i - 1) + j) * 2);//  ((i - 1) * colrows + (j - 1)) * 2;
            using (var binreader = new BinaryReader2(stream))
            {
                var point =  binreader.ReadAt(pos);
                return point;
            }

          


        }

        public static async Task<IEnumerable<Coordinates>> GetElevations(IEnumerable<Coordinates> points)
        {
            foreach (var item in points)
            {
                item.Elevation = await GetElevation(item.Latitude, item.Longitude);
            }
            return points;
        }
    }


    public class BinaryReader2 : BinaryReader
    {
        private byte[] a16 = new byte[2];
        private byte[] a32 = new byte[4];
        private byte[] a64 = new byte[8];
        public BinaryReader2(System.IO.Stream stream) : base(stream) { }
        public override int ReadInt32()
        {
            a32 = base.ReadBytes(4);
            Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }
        public Int16 ReadInt16()
        {
            a16 = base.ReadBytes(2);
            Array.Reverse(a16);
            return BitConverter.ToInt16(a16, 0);
        }
        public Int64 ReadInt64()
        {
            a64 = base.ReadBytes(8);
            Array.Reverse(a64);
            return BitConverter.ToInt64(a64, 0);
        }
        public UInt32 ReadUInt32()
        {
            a32 = base.ReadBytes(4);
            Array.Reverse(a32);
            return BitConverter.ToUInt32(a32, 0);
        }

        internal int ReadAt(int pos)
        {
            //var point = base.BaseStream.Length - pos;
            base.BaseStream.Seek(pos, SeekOrigin.Begin);
            return (int)ReadInt16();
        }
    }


    public class GeoAngle
    {
        public bool IsNegative { get; set; }
        public int Degrees { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }
        public double AllSeconds { get; private set; }

        public static GeoAngle FromDouble(double angleInDegrees)
        {
            //ensure the value will fall within the primary range [-180.0..+180.0]
            while (angleInDegrees < -180.0)
                angleInDegrees += 360.0;

            while (angleInDegrees > 180.0)
                angleInDegrees -= 360.0;

            var result = new GeoAngle();

            //switch the value to positive
            result.IsNegative = angleInDegrees < 0;
            angleInDegrees = Math.Abs(angleInDegrees);

            //gets the degree
            result.Degrees = (int)Math.Floor(angleInDegrees);
            var delta = angleInDegrees - result.Degrees;

            //gets minutes and seconds
            var seconds = (int)Math.Floor(3600.0 * delta);
            result.Seconds = seconds % 60;
            result.Minutes = (int)Math.Floor(seconds / 60.0);
            delta = delta * 3600.0 - seconds;

            //gets fractions
            result.Milliseconds = (int)(1000.0 * delta);
            result.AllSeconds = double.Parse($"{result.Seconds}.{result.Milliseconds}");

            return result;
        }



        public override string ToString()
        {
            var degrees = this.IsNegative
                ? -this.Degrees
                : this.Degrees;

            return string.Format(
                "{0}° {1:00}' {2:00}\"",
                degrees,
                this.Minutes,
                this.Seconds);
        }



        public string ToString(string format)
        {
            switch (format)
            {
                case "NS":
                    return string.Format(
                        "{0}° {1:00}' {2:00}\".{3:000} {4}",
                        this.Degrees,
                        this.Minutes,
                        this.Seconds,
                        this.Milliseconds,
                        this.IsNegative ? 'S' : 'N');

                case "WE":
                    return string.Format(
                        "{0}° {1:00}' {2:00}\".{3:000} {4}",
                        this.Degrees,
                        this.Minutes,
                        this.Seconds,
                        this.Milliseconds,
                        this.IsNegative ? 'W' : 'E');

                default:
                    throw new NotImplementedException();
            }
        }
    }

}
