using System;
using System.Linq;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace NexGenRoadLoader.commands
{
    public static class InsertFeatureIntoFeatureClass
    {
        public static void Execute(IFeature utransFeature, IFeatureClass nextGenFeatureClass, IFeatureClass zipsFC, IFeatureClass muniFC, IFeatureClass countiesFC, IFeatureClass addrSystemFC)
        {
            try
            {
                var newNexGenFeature = nextGenFeatureClass.CreateFeature();
                var utransShape = utransFeature.ShapeCopy;

                newNexGenFeature.Shape = utransShape;

                // Set values for fields that don't need spatial assigning.
                string STATUS = utransFeature.get_Value(utransFeature.Fields.FindField("STATUS")).ToString().Trim();
                string CARTOCODE = utransFeature.get_Value(utransFeature.Fields.FindField("CARTOCODE")).ToString().Trim();
                int FROMADDR_R = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("R_F_ADD")));
                int TOADDR_R = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("R_T_ADD")));
                int FROMADDR_L = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("L_F_ADD")));
                int TOADDR_L = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("L_T_ADD")));
                string PREDIR = utransFeature.get_Value(utransFeature.Fields.FindField("PREDIR")).ToString().Trim();
                string NAME = utransFeature.get_Value(utransFeature.Fields.FindField("STREETNAME")).ToString().Trim();
                string POSTTYPE = utransFeature.get_Value(utransFeature.Fields.FindField("STREETTYPE")).ToString().Trim();
                string POSTDIR = utransFeature.get_Value(utransFeature.Fields.FindField("SUFDIR")).ToString().Trim();
                string AN_NAME = utransFeature.get_Value(utransFeature.Fields.FindField("ACSNAME")).ToString().Trim();
                string AN_POSTDIR = utransFeature.get_Value(utransFeature.Fields.FindField("ACSSUF")).ToString().Trim();
                string A1_NAME = utransFeature.get_Value(utransFeature.Fields.FindField("ALIAS1")).ToString().Trim();
                string A1_POSTTYPE = utransFeature.get_Value(utransFeature.Fields.FindField("ALIAS1TYPE")).ToString().Trim();
                string A1_POSTDIR = "";
                string A1_PREDIR = "";
                string A2_NAME = utransFeature.get_Value(utransFeature.Fields.FindField("ALIAS2")).ToString().Trim();
                string A2_POSTTYPE = utransFeature.get_Value(utransFeature.Fields.FindField("ALIAS2TYPE")).ToString().Trim();
                string A2_POSTDIR = "";
                string A2_PREDIR = "";
                string STATE_L = "UT";
                string STATE_R = "UT";
                string ONEWAY = utransFeature.get_Value(utransFeature.Fields.FindField("ONEWAY")).ToString().Trim();
                string VERT_LEVEL = utransFeature.get_Value(utransFeature.Fields.FindField("VERTLEVEL")).ToString().Trim();
                int SPEED_LMT = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("SPEED")));
                string ACCESSCODE = utransFeature.get_Value(utransFeature.Fields.FindField("ACCESS")).ToString().Trim();
                string DOT_HWYNAM = utransFeature.get_Value(utransFeature.Fields.FindField("HWYNAME")).ToString().Trim();
                string DOT_RTNAME = utransFeature.get_Value(utransFeature.Fields.FindField("DOT_RTNAME")).ToString().Trim();
                string DOT_RTPART = utransFeature.get_Value(utransFeature.Fields.FindField("DOT_RTPART")).ToString().Trim();
                int DOT_F_MILE = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("DOT_F_MILE")));
                int DOT_T_MILE = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("DOT_T_MILE")));

                // Concatinate FullName field
                string FULLNAME = String.Empty;
                if (NAME.Any(x => !char.IsLetter(x))) // True if it doesn't contain letters.
                {
                    // ACS FULLNAME
                    FULLNAME = NAME + " " + POSTDIR;
                }
                else
                {
                    // ALPHA FULLNAME
                    // CHECK IF HIGHWAY, IF SO..
                    if (NAME.Contains("HIGHWAY "))
                    {
                        if (DOT_HWYNAM != "")
                        {
                            FULLNAME = DOT_HWYNAM;   
                        }
                        else
                        {
                            FULLNAME = NAME;
                            // Replace HIGHWAY WITH HWY.
                            FULLNAME = FULLNAME.Replace("HIGHWAY", "HWY");
                        }
                    }
                    else
                    {
                        FULLNAME = NAME + " " + POSTTYPE;                             
                    }
                }

                // Get values for the spatial fields by calling the method to spatially assign it.
                using (var spatialValues = GetSpatialFieldValues.Execute(utransFeature, zipsFC, muniFC, countiesFC, addrSystemFC)){
                
                    // Populate the non-spatial NextGenRoads' fields.
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("STATUS"), STATUS);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("CARTOCODE"), CARTOCODE);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("FULLNAME"), FULLNAME);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("FROMADDR_R"), FROMADDR_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("TOADDR_R"), TOADDR_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("FROMADDR_L"), FROMADDR_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("TOADDR_L"), TOADDR_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("PREDIR"), PREDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("NAME"), NAME);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("POSTTYPE"), POSTTYPE);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("POSTDIR"), POSTDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("AN_NAME"), AN_NAME);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("AN_POSTDIR"), AN_POSTDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A1_NAME"), A1_NAME);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A1_POSTTYPE"), A1_POSTTYPE);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A1_POSTDIR"), A1_POSTDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A1_PREDIR"), A1_PREDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A2_NAME"), A2_NAME);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A2_POSTTYPE"), A2_POSTTYPE);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A2_POSTDIR"), A2_POSTDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("A2_PREDIR"), A2_PREDIR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("STATE_L"), STATE_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("STATE_R"), STATE_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ONEWAY"), ONEWAY);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("VERT_LEVEL"), VERT_LEVEL);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("SPEED_LMT"), SPEED_LMT);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ACCESSCODE"), ACCESSCODE);    
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_HWYNAM"), DOT_HWYNAM);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_RTNAME"), DOT_RTNAME);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_RTPART"), DOT_RTPART);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_F_MILE"), DOT_F_MILE);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_T_MILE"), DOT_T_MILE);

                    // Populate spatial assigned NextGenRoads' fields.
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ZIPCODE_L"), spatialValues.Zip_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ZIPCODE_R"), spatialValues.Zip_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("POSTCOMM_L"), spatialValues.PostalComm_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("POSTCOMM_R"), spatialValues.PostalComm_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("INCMUNI_L"), spatialValues.IncMuni_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("INCMUNI_R"), spatialValues.IncMuni_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("COUNTY_L"), spatialValues.County_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("COUNTY_R"), spatialValues.County_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ADDRSYS_L"), spatialValues.AddrSystem_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ADDRSYS_R"), spatialValues.AddrSystem_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("QUADRANT"), spatialValues.AddrSystemQuad_L); // we have a AddrSystemQuad_R i just need a NG schema roads field to populate.


                    // Store the feature.
                    newNexGenFeature.Store();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "There was an error with InsertFeatureInto method." +
                    ex.Message + " " + ex.Source + " " + ex.InnerException + " " + ex.HResult + " " + ex.StackTrace + " " + ex);
                Console.ReadLine();
            }
        }
    }
}
