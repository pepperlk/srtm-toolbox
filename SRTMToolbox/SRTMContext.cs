using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SRTMToolbox
{
    public class SRTMContext
    {
        public async Task Cache(string srtmLocation)
        {
            Trace.WriteLine($"Getting {srtmLocation}");
            var localendpoint = SRTM.Endpoints.FirstOrDefault(e => e.Type == SRTMEndpointType.Local);
            var zipfile = "";

            var cachedir = Path.Combine(SRTM.WorkingDirectory.Directory.FullName, "op_cache");
            var outdir = Path.GetDirectoryName(SRTM.WorkingDirectory.Directory.FullName + srtmLocation.Replace('/', '\\'));
            var outfile = SRTM.WorkingDirectory.Directory.FullName + srtmLocation.Replace('/', '\\').Replace(".zip", "");
            if (File.Exists(outfile)) { return; }
            if (!Directory.Exists(cachedir)) { Directory.CreateDirectory(cachedir); }
            if (localendpoint != null)
            {

                srtmLocation = srtmLocation.Replace('/', '\\');

                // copy to localopcache
                var filepath = Path.Combine(localendpoint.Location, srtmLocation);
                Trace.WriteLine($"Copy from {localendpoint.Name}");

                var savepath = cachedir + srtmLocation.Replace('/', '\\');
                if (!Directory.Exists(Path.GetDirectoryName(savepath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savepath));
                }

                File.Copy(filepath, savepath);
                zipfile = savepath;
            }

            var webendpoint = SRTM.Endpoints.FirstOrDefault(e => e.Type == SRTMEndpointType.Web);
            if (webendpoint != null)
            {
                var cli = new HttpClient();

                var savepath = cachedir + srtmLocation.Replace('/', '\\');
                if (!File.Exists(savepath))
                {



                    var downloadLink = webendpoint.Location.TrimEnd('/') + "/" + srtmLocation.TrimStart('/');
                    Trace.WriteLine($"Download from {webendpoint.Name}: {downloadLink}");
                    var stream = await cli.GetStreamAsync(downloadLink);


                    Trace.WriteLine($"Saving file to {savepath}");
                    if (!Directory.Exists(Path.GetDirectoryName(savepath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(savepath));
                        Trace.WriteLine($"Creating dir{Path.GetDirectoryName(savepath)}");
                    }

                    using (var fileStream = File.Create(savepath))
                    {
                        //stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }

                    Trace.WriteLine($"File saved to {savepath}");
                }
                zipfile = savepath;
            }
            Trace.WriteLine($"Unzip hgt {zipfile}");
            if (!Directory.Exists(outdir))
            {
                Directory.CreateDirectory(outdir);
            }
            try
            {
                using (ZipFile zf = new ZipFile(zipfile))
                {
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue;           // Ignore directories
                        }
                        String entryFileName = zipEntry.Name;
                        // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        byte[] buffer = new byte[4096];     // 4K is optimum
                        Stream zipStream = zf.GetInputStream(zipEntry);

                        // Manipulate the output filename here as desired.
                        String fullZipToPath = Path.Combine(outdir, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                        // of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }



        }

        public IEnumerable<Coordinates> GetPointsBetween(double lat1, double lng1, double lat2, double lng2, int resolutionMeters = 90 )
        {
            var targetcoord = new Coordinates(lat2, lng2);
            var startcoord = new Coordinates(lat1, lng1);


            var distance = startcoord
                .DistanceTo(
                    targetcoord,
                    UnitOfLength.Kilometers
                ) * 1000;


            var coords = new List<Coordinates>();
            coords.Add(startcoord);
            var currentcoord = startcoord;
            //var points = (int)(distance / 90);
            //for (int i = 0; i < points; i++)
            //{
            while (true) { 
                var bearing = currentcoord.DegreeBearing(targetcoord);
                var nextpoint = currentcoord.FindPointAtDistanceFrom(bearing, resolutionMeters);

                if(currentcoord.DistanceTo(targetcoord) * 1000 < resolutionMeters * 2)
                {
                    break;
                }

                coords.Add(nextpoint);
                currentcoord = nextpoint;

            }

            coords.Add(targetcoord);

            var teststring = $"[ {string.Join(",", coords)} ]";



            return coords;
        }

        public SRTMEndpoint GetBestEndpoint()
        {
            var localendpoint = SRTM.Endpoints.FirstOrDefault(e => e.Type == SRTMEndpointType.Local);
            if (localendpoint != null)
            {
                return localendpoint;
            }
            var webendpoint = SRTM.Endpoints.FirstOrDefault(e => e.Type == SRTMEndpointType.Web);
            if (webendpoint != null)
            {
                return webendpoint;
            }
            return null;
        }


        public async Task<SRTMFile> FindSRTMFile(string file)
        {
            var locations = new List<string>();
            locations.Add("/SRTM1/Region_01/" + file + ".zip");
            locations.Add("/SRTM1/Region_02/" + file + ".zip");
            locations.Add("/SRTM1/Region_03/" + file + ".zip");
            locations.Add("/SRTM1/Region_04/" + file + ".zip");
            locations.Add("/SRTM1/Region_05/" + file + ".zip");
            locations.Add("/SRTM1/Region_06/" + file + ".zip");
            locations.Add("/SRTM1/Region_07/" + file + ".zip");

            locations.Add("/SRTM3/Africa/" + file + ".zip");
            locations.Add("/SRTM3/Australia/" + file + ".zip");
            locations.Add("/SRTM3/Eurasia/" + file + ".zip");
            locations.Add("/SRTM3/Islands/" + file + ".zip");
            locations.Add("/SRTM3/North_America/" + file + ".zip");
            locations.Add("/SRTM3/South_America/" + file + ".zip");

            // check for local


            foreach (var l in locations)
            {
                if (File.Exists(SRTM.WorkingDirectory.Directory.FullName + l.Replace('/', '\\').Replace(".zip", "")))
                {
                    return new SRTMFile(l);
                }
            }



            var endpoint = GetBestEndpoint();
            switch (endpoint.Type)
            {
                case SRTMEndpointType.Web:


                    foreach (var l in locations)
                    {


                        try
                        {

                            var downloadLink = endpoint.Location.TrimEnd('/') + "/" + l.TrimStart('/');
                            HttpClient httpClient = new HttpClient();

                            HttpRequestMessage request =
                               new HttpRequestMessage(HttpMethod.Head,
                                  new Uri(downloadLink));

                            HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                            if (response.IsSuccessStatusCode)
                            {
                                return new SRTMFile(l);
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }

                    break;
                case SRTMEndpointType.Local:
                    break;
                default:
                    break;
            }
            return null;
        }

        public string GetSRTMFileForLatLng(double lat, double lng)
        {

            var latdir = "N";
            if (lat < 0)
            {
                latdir = "S";
                lat = lat - 1;
            }
            var lngdir = "E";
            if (lng < 0)
            {
                lngdir = "W";
                lng = lng - 1;
            }


            var srtmFilename = $"{latdir}{$"{Math.Abs((int)lat)}".PadLeft(2, '0')}{lngdir}{$"{Math.Abs((int)lng)}".PadLeft(3, '0')}.hgt";


            return srtmFilename;
        }
    }


    public class Coordinates
    {
        public double Latitude { get;  set; }
        public double Longitude { get;  set; }
        public double Elevation { get;  set; }

        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return $"[ {Longitude}, {Latitude} ]";
        }
    }
    public static class CoordinatesDistanceExtensions
    {
        public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates)
        {
            return DistanceTo(baseCoordinates, targetCoordinates, UnitOfLength.Kilometers);
        }


        public static Coordinates FindPointAtDistanceFrom(this Coordinates startPoint, double initialBearingRadians, double distanceMetres)
        {
            const double EarthRadius = 6378137.0;
            double latA = ToRad(startPoint.Latitude);// * DegreesToRadians;
            double lonA = ToRad(startPoint.Longitude);// source.Lon * DegreesToRadians;
            double angularDistance = distanceMetres / EarthRadius;
            double trueCourse = ToRad(initialBearingRadians);// * DegreesToRadians;

            double lat = Math.Asin(Math.Sin(latA) * Math.Cos(angularDistance) + Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));

            double dlon = Math.Atan2(Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA), Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));
            double lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;


            return new Coordinates(ToDegrees(lat), ToDegrees(lon));
            //return new LatLon(lat * RadiansToDegrees, lon * RadiansToDegrees);



            //const double radiusEarthKilometres = 6371.01 * 1000;
            //var distRatio = distanceMetres / radiusEarthKilometres;
            //var distRatioSine = Math.Sin(distRatio);
            //var distRatioCosine = Math.Cos(distRatio);

            //var startLatRad = ToRad(startPoint.Latitude);
            //var startLonRad = ToRad(startPoint.Longitude);

            //var startLatCos = Math.Cos(startLatRad);
            //var startLatSin = Math.Sin(startLatRad);

            //var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));

            //var endLonRads = startLonRad
            //    + Math.Atan2(
            //        Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
            //        distRatioCosine - startLatSin * Math.Sin(endLatRads));

            //return new Coordinates(ToDegrees(endLatRads), ToDegrees(endLonRads));

        }


        public static double DegreeBearing(this Coordinates baseCoordinates, Coordinates targetCoordinates)
        {

            double lat1 = baseCoordinates.Latitude;
            double lon1 = baseCoordinates.Longitude;
            double lat2 = targetCoordinates.Latitude;
            double lon2 = targetCoordinates.Longitude;

            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        public static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        public static double DistanceTo(this Coordinates baseCoordinates, Coordinates targetCoordinates, UnitOfLength unitOfLength)
        {
            var baseRad = Math.PI * baseCoordinates.Latitude / 180;
            var targetRad = Math.PI * targetCoordinates.Latitude / 180;
            var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return unitOfLength.ConvertFromMiles(dist);
        }
    }

    public class UnitOfLength
    {
        public static UnitOfLength Kilometers = new UnitOfLength(1.609344);
        public static UnitOfLength NauticalMiles = new UnitOfLength(0.8684);
        public static UnitOfLength Miles = new UnitOfLength(1);

        private readonly double _fromMilesFactor;

        private UnitOfLength(double fromMilesFactor)
        {
            _fromMilesFactor = fromMilesFactor;
        }

        public double ConvertFromMiles(double input)
        {
            return input * _fromMilesFactor;
        }
    }
}
