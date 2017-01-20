using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SRTMToolbox.Test
{
    [TestClass]
    public class SRTMTests
    {
        [TestMethod]
        public void SetupConfig()
        {
            Assert.IsTrue(SRTM.WorkingDirectory.ToString() == Directory.GetCurrentDirectory());
            Assert.IsTrue(SRTM.Endpoints.Any(e => e.Name == "NASA 2.1"));



        }




        [TestMethod]
        public async Task CacheHgtFile()
        {

            await SRTM.Context.Cache("/SRTM1/Region_01/N38W112.hgt.zip");
            Assert.IsTrue(File.Exists(SRTM.WorkingDirectory.Directory + "\\SRTM1\\Region_01\\N38W112.hgt"));
        }



        [TestMethod]
        public async Task GetSrtmFiles()
        {
            var lat = 41.1400;
            var lng = -104.8202;

            var file = SRTM.Context.GetSRTMFileForLatLng(lat, lng);


            var srtmFile = await SRTM.Context.FindSRTMFile(file);

            var stream = await srtmFile.OpenRead();





        }


        [TestMethod]
        public async Task GetSrtmElevation()
        {
            var lat = 41.1400;
            var lng = -104.8202;
            var ele = 1869;


            //var lng = 14.9198269444;
            //var lat = 50.4163577778;

            var elevation = await SRTM.GetElevation(lat, lng);
            Assert.IsTrue(ele == elevation);
        }


        [TestMethod]
        public async Task GetGetLineBetween2Points()
        {
            var lat1 = 41.16043385872706;
            var lng1 = -104.80888366699219;

            var lat2 = 41.15991690216381;
            var lng2 = -104.71738815307617;

            var points = SRTM.Context.GetPointsBetween(lat1, lng1, lat2, lng2);

            var eleLine = await SRTM.GetElevations(points);

            var csvTest = $"{string.Join(",", eleLine.Select(e=> e.Elevation))}";



            //var lng = 14.9198269444;
            //var lat = 50.4163577778;

            //var elevation = await SRTM.GetElevation(lat, lng);
            //Assert.IsTrue(ele == elevation);
        }


    }
}
