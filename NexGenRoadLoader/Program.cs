using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using NexGenRoadLoader.contracts;
using NexGenRoadLoader.loaders;
using NexGenRoadLoader.models;
using NexGenRoadLoader.services;

namespace NexGenRoadLoader
{
    internal class Program
    {
        private static readonly LicenseInitializer LicenseInitializer = new LicenseInitializer();

        private static void Main(string[] args)
        {
            CliOptions options;

            try
            {
                options = ArgParserService.Parse(args);
                if (options == null)
                {
                    return;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Write("nexgen loader: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("press any key to continue");
                Console.ReadKey();

                return;
            }

            const string roadsFeatureClassName = "UTRANS.TRANSADMIN.StatewideStreets";
            const string sgidZipCodes = "SGID10.BOUNDARIES.ZipCodes";
            const string sgidMuniBoundaries = "SGID10.BOUNDARIES.Municipalities";
            const string sgidAddressSystems = "SGID10.LOCATION.AddressSystemQuadrants";
            const string sgidCounties = "SGID10.BOUNDARIES.Counties";
            const string sgidMetroTownships = "SGID10.BOUNDARIES.MetroTownships";

            var start = Stopwatch.StartNew();

            //ESRI License Initializer generated code
            //try to check out an arcinfo license
            if (!LicenseInitializer.InitializeApplication(new[] { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
                new esriLicenseExtensionCode[] { }))
            {
                //if the license could not be initalized, shut it down
                Console.WriteLine(LicenseInitializer.LicenseMessage());
                Console.WriteLine("This application could not initialize with the correct ArcGIS license and will shutdown.");

                LicenseInitializer.ShutdownApplication();
                return;
            }
            Console.WriteLine("{0} Checked out a license", start.Elapsed);

            using (var releaser = new ComReleaser())
            {
                // GET UTRANS WORKSPACE
                IWorkspace utrans;
                try
                {
                    Console.WriteLine("{1} Connecting to: {0}", options.SdeConnectionPath, start.Elapsed);

                    utrans = WorkspaceService.GetSdeWorkspace(options.SdeConnectionPath);
                    releaser.ManageLifetime(utrans);
                }
                catch (COMException e)
                {
                    Console.Write("nextgen loader: ");
                    Console.WriteLine(e.Message);

                    Console.ReadKey();
                    return;
                }

                // GET SGID WORKSPACE
                IWorkspace sgid;
                try
                {
                    Console.WriteLine("{1} Connecting to: {0}", options.SdeConnectionPath2, start.Elapsed);

                    sgid = WorkspaceService.GetSdeWorkspace(options.SdeConnectionPath2);
                    releaser.ManageLifetime(sgid);
                }
                catch (COMException e)
                {
                    Console.Write("nextgen loader: ");
                    Console.WriteLine(e.Message);

                    Console.ReadKey();
                    return;
                }


                // GET UTRANS ROADS FC
                var utransFeatureWorkspace = (IFeatureWorkspace)utrans;
                releaser.ManageLifetime(utransFeatureWorkspace);
                // get the source roads feature class (UTRANS)
                var roads = utransFeatureWorkspace.OpenFeatureClass(roadsFeatureClassName);
                releaser.ManageLifetime(roads);


                //using (var releaser2 = new ComReleaser())
                //{
                    // GET SGID ZIPCODES
                    var sgidFeatureWorkspace = (IFeatureWorkspace)sgid;
                    releaser.ManageLifetime(sgidFeatureWorkspace);
                    
                    // get the source roads feature class (sgid)
                    var zips = sgidFeatureWorkspace.OpenFeatureClass(sgidZipCodes);
                    releaser.ManageLifetime(zips);
                    
                    // GET SGID MUNIS
                    var muni = sgidFeatureWorkspace.OpenFeatureClass(sgidMuniBoundaries);
                    releaser.ManageLifetime(muni);
                    
                    // GET SGID COUNTIES
                    var counties = sgidFeatureWorkspace.OpenFeatureClass(sgidCounties);
                    releaser.ManageLifetime(counties);
                    
                    // GET SGID ADDRESSSYSTEMS
                    var addrSystems = sgidFeatureWorkspace.OpenFeatureClass(sgidAddressSystems);
                    releaser.ManageLifetime(addrSystems);

                    // GET SGID METROTOWNSHIPS
                    var metroTwnShp = sgidFeatureWorkspace.OpenFeatureClass(sgidMetroTownships);
                    releaser.ManageLifetime(metroTwnShp);

                //}


                ILoader loader;
                switch (options.OutputType)
                {
                    case OutputType.NextGenRoads:
                    {
                        // Create an new instance of the NexGenRoadsLoader
                        loader = new NextGenLoader(roads, zips, muni, counties, addrSystems, metroTwnShp, options); 
                        break;
                    }

                    default:
                    {
                        return;
                    }
                }

                var output = loader.GetOutputWorkspace();
                loader.Load(output);
            }
        }
    }
}
