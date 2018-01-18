using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

namespace NexGenRoadLoader.commands
{
    public static class InsertFeatureIntoFeatureClass
    {
        public static void Execute(IFeature utransFeature, IFeatureClass nextGenFeatureClass, IFeatureClass zipsFC, IFeatureClass muniFC, IFeatureClass countiesFC, IFeatureClass addrSystemFC, IFeatureClass metroTownship, StreamWriter streamWriter)
        {

            var newNexGenFeature = nextGenFeatureClass.CreateFeature();
            try
            {
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
                // check for dbnull values.
                var SPEED_LMT = utransFeature.get_Value(utransFeature.Fields.FindField("SPEED"));
                //if (SPEED_LMT != null)
                //{
                //    // Convert the value to int
                //    SPEED_LMT = Convert.ToInt32(utransFeature.get_Value(utransFeature.Fields.FindField("SPEED")));  
                //}
                string ACCESSCODE = utransFeature.get_Value(utransFeature.Fields.FindField("ACCESS")).ToString().Trim();
                string DOT_HWYNAM = utransFeature.get_Value(utransFeature.Fields.FindField("HWYNAME")).ToString().Trim();
                string DOT_RTNAME = utransFeature.get_Value(utransFeature.Fields.FindField("DOT_RTNAME")).ToString().Trim();
                string DOT_RTPART = utransFeature.get_Value(utransFeature.Fields.FindField("DOT_RTPART")).ToString().Trim();
                var DOT_F_MILE = utransFeature.get_Value(utransFeature.Fields.FindField("DOT_F_MILE"));
                var DOT_T_MILE = utransFeature.get_Value(utransFeature.Fields.FindField("DOT_T_MILE"));
                //string DOT_FCLASS = utransFeature.get_Value(utransFeature.Fields.FindField("CLASS")).ToString().Trim();
                string DOT_SRFTYP = utransFeature.get_Value(utransFeature.Fields.FindField("SURFTYPE")).ToString().Trim();
                string DOT_CLASS = utransFeature.get_Value(utransFeature.Fields.FindField("CLASS")).ToString().Trim();
                //var DOT_AADT; // int
                string DOT_AADTYR = "";
                //var DOT_THRULANES; // small int
                string BIKE_L = utransFeature.get_Value(utransFeature.Fields.FindField("BIKE_L")).ToString().Trim();
                string BIKE_R = utransFeature.get_Value(utransFeature.Fields.FindField("BIKE_R")).ToString().Trim();
                string BIKE_PLN_L = utransFeature.get_Value(utransFeature.Fields.FindField("BIKE_STATUS")).ToString().Trim();
                string BIKE_PLN_R = utransFeature.get_Value(utransFeature.Fields.FindField("BIKE_STATUS")).ToString().Trim();
                string BIKE_NOTES = utransFeature.get_Value(utransFeature.Fields.FindField("BIKE_NOTES")).ToString().Trim();
                string UNIQUE_ID = utransFeature.get_Value(utransFeature.Fields.FindField("UNIQUE_ID")).ToString().Trim();
                string LOCAL_UID = utransFeature.get_Value(utransFeature.Fields.FindField("COUNIQUE")).ToString().Trim();
                string UTAHRD_UID = "";
                string SOURCE = utransFeature.get_Value(utransFeature.Fields.FindField("SOURCE")).ToString().Trim();
                var UPDATED = utransFeature.get_Value(utransFeature.Fields.FindField("MODIFYDATE")); //date field
                string CREATOR = utransFeature.get_Value(utransFeature.Fields.FindField("CREATOR")).ToString().Trim();
                string EDITOR = utransFeature.get_Value(utransFeature.Fields.FindField("EDITOR")).ToString().Trim();
                var CREATED = utransFeature.get_Value(utransFeature.Fields.FindField("CREATE_DATE"));
                string UTRANS_NOTES = utransFeature.get_Value(utransFeature.Fields.FindField("NOTES")).ToString().Trim();


                //var EFFECTIVE; //date field
                //var EXPIRE; //date field
                string CUSTOMTAGS = "";


                // Concatinate FullName field
                string FULLNAME = String.Empty;
                if (NAME != "")
                {
                    //if (NAME.Any(x => !char.IsLetter(x))) // True if it doesn't contain letters.  - old code
                    // this was being coded via lambda expresstion but found that it was returning false values so i found this regex expression that does the trick.
                    if (Regex.IsMatch(NAME, @"[a-zA-Z]")) // true if NAME conatins at least one letter
                    {
                        // ALPHA FULLNAME
                        // CHECK IF HIGHWAY, IF SO..
                        if (NAME.Contains("HIGHWAY "))
                        {
                            //if (DOT_HWYNAM != "")
                            //{
                                //FULLNAME = DOT_HWYNAM;
                            //}
                            //else
                            //{
                            FULLNAME = NAME;
                            // Replace HIGHWAY WITH HWY.
                            FULLNAME = FULLNAME.Replace("HIGHWAY", "HWY");
                            //}
                        }
                        else
                        {
                            FULLNAME = NAME + " " + POSTTYPE;
                        }
                    }
                    else
                    {
                        // ACS FULLNAME
                        FULLNAME = NAME + " " + POSTDIR;
                    }                    
                }


                // check for bad ACCESS code values in utrans before push
                if (ACCESSCODE != "A" && ACCESSCODE != "F" && ACCESSCODE != "G" && ACCESSCODE != "S" && ACCESSCODE != "T" && ACCESSCODE != "")
                {
                    // Must be a bad value...
                    if (ACCESSCODE == "2T")
                    {
                        // limited, two wheel drive
                        ACCESSCODE = "*";
                    }
                    else if (ACCESSCODE == "1")
                    {
                        // 1 = no restrictions
                        ACCESSCODE = "";
                    }
                    else
                    {
                        ACCESSCODE = "*";        
                    }
                }

                // Get values for the spatial fields by calling the method to spatially assign it.
                using (var spatialValues = GetSpatialFieldValues.Execute(utransFeature, zipsFC, muniFC, countiesFC, addrSystemFC, metroTownship)){

                    // Get values for fields that need aditional logic.
                    string DOT_OWN_L = "";
                    string DOT_OWN_R = "";
                    // Get first four characters from DOT_RTNAME and convert them int.
                    if (DOT_RTNAME.Length >= 4)
                    {
                        // Get first four characters
                        string DOT_RTNAME_First4string = DOT_RTNAME.Substring(0, 4);

                        // See if first four characters are int
                        //int DOT_RTNAME_Fisrt4int = Convert.ToInt32(DOT_RTNAME.Substring(0, 4));
                        int DOT_RTNAME_Fisrt4int;
                        bool first4isNumeric = int.TryParse(DOT_RTNAME_First4string, out DOT_RTNAME_Fisrt4int);

                        // If first four characters are int then...
                        if (first4isNumeric)
                        {
                            // first four are int.
                            // Get values for the DOT_OWN fields.
                            if (DOT_RTNAME_Fisrt4int >= 6 || DOT_RTNAME_Fisrt4int < 1000)
                            {
                                DOT_OWN_L = "UDOT";
                                DOT_OWN_R = "UDOT";
                            }
                            else if (DOT_RTNAME_Fisrt4int >= 1000 || DOT_RTNAME_Fisrt4int < 4000)
                            {
                                if (DOT_CLASS == "B")
                                {
                                    DOT_OWN_L = spatialValues.County_L;
                                    DOT_OWN_R = spatialValues.County_R;                           
                                }
                                else if (DOT_CLASS == "C")
                                {
                                    DOT_OWN_L = spatialValues.IncMuni_L;
                                    DOT_OWN_R = spatialValues.IncMuni_R;
                                }
                            }  
                        }
                        else
                        {
                            // first four are not int.
                            // check for know outliers
                            if (DOT_RTNAME_First4string == "089AP")
                            {
                                DOT_OWN_L = "UDOT";
                                DOT_OWN_R = "UDOT";                                  
                            }
                            else
                            {
                                DOT_OWN_L = "*";
                                DOT_OWN_R = "*";                                    
                            }
                        }
                    }

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
                    //newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_FCLASS"), DOT_FCLASS);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_SRFTYP"), DOT_SRFTYP);
                    if (DOT_CLASS.Length == 1)
                    {
                        newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_CLASS"), DOT_CLASS);                       
                    }
                    else if (DOT_CLASS.Length > 1)
                    {
                        newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_CLASS"), "*");                            
                    }
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_OWN_L"), DOT_OWN_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_OWN_R"), DOT_OWN_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("DOT_AADTYR"), DOT_AADTYR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("BIKE_L"), BIKE_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("BIKE_R"), BIKE_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("BIKE_PLN_L"), BIKE_PLN_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("BIKE_PLN_R"), BIKE_PLN_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("BIKE_NOTES"), BIKE_NOTES);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("UNIQUE_ID"), UNIQUE_ID);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("LOCAL_UID"), LOCAL_UID);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("UTAHRD_UID"), UTAHRD_UID);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("SOURCE"), SOURCE);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("UPDATED"), UPDATED);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("CUSTOMTAGS"), CUSTOMTAGS);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("CREATED"), CREATED);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("EDITOR"), EDITOR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("CREATOR"), CREATOR);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("UTRANS_NOTES"), UTRANS_NOTES);

                    // Populate spatial assigned NextGenRoads' fields.
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ZIPCODE_L"), spatialValues.Zip_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ZIPCODE_R"), spatialValues.Zip_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("POSTCOMM_L"), spatialValues.PostalComm_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("POSTCOMM_R"), spatialValues.PostalComm_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("INCMUNI_L"), spatialValues.IncMuni_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("INCMUNI_R"), spatialValues.IncMuni_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("UNINCCOM_L"), spatialValues.UnIncMuni_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("UNINCCOM_R"), spatialValues.UnIncMuni_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("COUNTY_L"), spatialValues.County_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("COUNTY_R"), spatialValues.County_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ADDRSYS_L"), spatialValues.AddrSystem_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("ADDRSYS_R"), spatialValues.AddrSystem_R);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("QUADRANT_L"), spatialValues.AddrSystemQuad_L);
                    newNexGenFeature.set_Value(newNexGenFeature.Fields.FindField("QUADRANT_R"), spatialValues.AddrSystemQuad_R);

                    // Log the OID to the console.
                    Console.WriteLine(utransFeature.get_Value(utransFeature.Fields.FindField("OBJECTID")).ToString());

                    // Store the feature.
                    newNexGenFeature.Store();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "There was an error with InsertFeatureInto method." +
                    ex.Message + " " + ex.Source + " " + ex.InnerException + " " + ex.HResult + " " + ex.StackTrace + " " + ex);
                //Console.ReadLine();
                streamWriter.WriteLine(utransFeature.get_Value(utransFeature.Fields.FindField("OBJECTID")).ToString() + "," + newNexGenFeature.get_Value(newNexGenFeature.Fields.FindField("OBJECTID")));
            }
        }
    }
}
