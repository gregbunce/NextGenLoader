using System;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using NexGenRoadLoader.commands;
using NexGenRoadLoader.contracts;
using NexGenRoadLoader.models;
using NexGenRoadLoader.services;

namespace NexGenRoadLoader.loaders
{
    public class NextGenLoader : ILoader
    {
        private readonly CliOptions _options;
        private readonly IFeatureClass _roads;
        private readonly IFeatureClass _zips;
        private readonly IFeatureClass _muni;
        private readonly IFeatureClass _counties;
        private readonly IFeatureClass _addrSystem;

        // The roads and clioptions get passed in here - this is the class constructor.
        public NextGenLoader(IFeatureClass source, IFeatureClass zips, IFeatureClass muni, IFeatureClass counties, IFeatureClass addrSys, CliOptions options)
        {
            _roads = source;
            _zips = zips;
            _muni = muni;
            _counties = counties;
            _addrSystem = addrSys;
            _options = options;
        }


        // This method is the worker that loads utrans roads into the new schema fgdb.
        public void Load(IWorkspace output)
        {
            Console.WriteLine("Begin creating loading roads to nexgen fgdb: " + DateTime.Now);

            try
            {
                var outputFeatureWorkspace = (IFeatureWorkspace)output;
                IFeatureClass outputFeatureClass = outputFeatureWorkspace.OpenFeatureClass("NextGenRoads");

                // Check if NextGenRoads has features, if so delete them (truncate table)
                if (outputFeatureClass.FeatureCount(null) != 0)
                {
                    // Truncate the table
                    ITableWrite2 tableWrite2 = outputFeatureClass as ITableWrite2;
                    tableWrite2.Truncate();
                }

                // Get feature cursor of utrans roads to loop through 
                const string getUtransRoads = "WHERE STREETNAME = 'MAIN'";

                var roadsFilter = new QueryFilter
                {
                    WhereClause = getUtransRoads
                };

                // create a ComReleaser for feature cursor's life-cycle management                
                using (var comReleaser = new ComReleaser())
                {
                    var roadsCursor = _roads.Search(roadsFilter, false);
                    comReleaser.ManageLifetime(roadsCursor);

                    // begin an edit session on the file geodatabase (maybe) that way we can roll back if it errors out
                    //outputEditWorkspace.StartEditing(false);
                    //outputEditWorkspace.StartEditOperation();

                    IFeature roadFeature;

                    // loop through the sgid roads' feature cursor
                    while ((roadFeature = roadsCursor.NextFeature()) != null)
                    {
                        InsertFeatureIntoFeatureClass.Execute(roadFeature, outputFeatureClass, _zips, _muni, _counties, _addrSystem);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Read();
                throw;
            }
        }


        // Get the filegeodatabase workspace.
        public IWorkspace GetOutputWorkspace()
        {
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
            IWorkspace workspace = workspaceFactory.OpenFromFile(_options.OutputGeodatabase + "NexGenRoadLoader.gdb", 0);
            //IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspace;
            return workspace;
        }
    }
}
