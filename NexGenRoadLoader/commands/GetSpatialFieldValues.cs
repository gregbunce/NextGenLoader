using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using NexGenRoadLoader.models;

namespace NexGenRoadLoader.commands
{
    class GetSpatialFieldValues
    {
        public static SpatialFieldValues Execute(IFeature utransFeature, IFeatureClass zipsFeatClass, IFeatureClass muniFeatClass, IFeatureClass countyFeatClass, IFeatureClass addrSysFeatClass, IFeatureClass metroTwnShp)
        {
            try
            {
                // Instantiate a new fields value object.
                var spatialFieldValues = new SpatialFieldValues();
                //spatialFieldValues.Muni_L = "Lefty";
                //spatialFieldValues.Muni_R = "Righty";


                // Get the right and left offset points for the utrans feature for sgid spatial queries.
                //get the midpoint of the line segment for doing spatial queries (intersects)
                IGeometry arcUtransEdits_geometry = utransFeature.ShapeCopy;
                IPolyline arcUtransEdits_polyline = arcUtransEdits_geometry as IPolyline;
                
                // Get the midpoint of the line, if needed on fields w/o right/left values.
                //IPoint arcUtransEdits_midPoint = new ESRI.ArcGIS.Geometry.Point();
                //get the midpoint of the line, pass it into a point
                //arcUtransEdits_polyline.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, arcUtransEdits_midPoint);
                
                // Use iconstructpoint.constructoffset method to offset the midpoint of the line.
                IConstructPoint constructionPoint_positiveRight = new ESRI.ArcGIS.Geometry.PointClass();
                IConstructPoint constructionPoint_negativeLeft = new ESRI.ArcGIS.Geometry.PointClass();

                // call offset mehtod to get a point along the curve's midpoint - offsetting in the postive position (esri documentation states that positive offset will always return point on the right side of the curve)
                constructionPoint_positiveRight.ConstructOffset(arcUtransEdits_polyline, esriSegmentExtension.esriNoExtension, 0.5, true, 15);  // 10 meters is about 33 feet (15 is about 50 feet)
                IPoint outPoint_posRight = constructionPoint_positiveRight as ESRI.ArcGIS.Geometry.IPoint;

                // call offset mehtod to get a point along the curve's midpoint - offsetting in the negative position (esri documentation states that negative offset will always return point on the left-side of curve)
                constructionPoint_negativeLeft.ConstructOffset(arcUtransEdits_polyline, esriSegmentExtension.esriNoExtension, 0.5, true, -15);  // -10 meters is about -33 feet (15 is about 50 feet)
                IPoint outPoint_negLeft = constructionPoint_negativeLeft as ESRI.ArcGIS.Geometry.IPoint;



                // LEFT - ZIP & USPS COMMUNITY
                ISpatialFilter arcSpatialFilter_leftZip = new SpatialFilter();
                arcSpatialFilter_leftZip.Geometry = outPoint_negLeft;
                arcSpatialFilter_leftZip.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor arcZipCursor_left = zipsFeatClass.Search(arcSpatialFilter_leftZip, false);
                IFeature arcFeatureZip_left = arcZipCursor_left.NextFeature();
                if (arcFeatureZip_left != null)
                {
                    spatialFieldValues.Zip_L =
                        arcFeatureZip_left.get_Value(arcFeatureZip_left.Fields.FindField("ZIP5")).ToString().Trim();
                    spatialFieldValues.PostalComm_L = arcFeatureZip_left
                        .get_Value(arcFeatureZip_left.Fields.FindField("NAME")).ToString().Trim().ToUpper();
                }
                else
                {
                    spatialFieldValues.Zip_L = "NA";
                    spatialFieldValues.PostalComm_L = "NA";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcZipCursor_left);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_leftZip);


                // RIGHT ZIP & USPS COMMUNITY
                ISpatialFilter arcSpatialFilter_rightZip = new SpatialFilter();
                arcSpatialFilter_rightZip.Geometry = outPoint_posRight;
                arcSpatialFilter_rightZip.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor arcZipCursor_right = zipsFeatClass.Search(arcSpatialFilter_rightZip, false);
                IFeature arcFeatureZip_right = arcZipCursor_right.NextFeature();
                if (arcFeatureZip_right != null)
                {
                    spatialFieldValues.Zip_R =
                        arcFeatureZip_left.get_Value(arcFeatureZip_left.Fields.FindField("ZIP5")).ToString().Trim();
                    spatialFieldValues.PostalComm_R = arcFeatureZip_left
                        .get_Value(arcFeatureZip_left.Fields.FindField("NAME")).ToString().Trim().ToUpper();
                }
                else
                {
                    spatialFieldValues.Zip_R = "NA";
                    spatialFieldValues.PostalComm_R = "NA";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcZipCursor_right);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_leftZip);


                // LEFT - INCORPORATED COMM
                ISpatialFilter arcSpatialFilter_leftIncComm = new SpatialFilter();
                arcSpatialFilter_leftIncComm.Geometry = outPoint_negLeft;
                arcSpatialFilter_leftIncComm.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor arcIncCommCursor_left = muniFeatClass.Search(arcSpatialFilter_leftIncComm, false);
                IFeature arcFeatureIncComm_left = arcIncCommCursor_left.NextFeature();
                if (arcFeatureIncComm_left != null)
                {
                    spatialFieldValues.IncMuni_L =
                        arcFeatureIncComm_left.get_Value(arcFeatureIncComm_left.Fields.FindField("NAME")).ToString().Trim().ToUpper();
                }
                else
                {
                    spatialFieldValues.IncMuni_L = "";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcIncCommCursor_left);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_leftIncComm);

                // RIGHT - INCORPORATED COMM
                ISpatialFilter arcSpatialFilter_rightIncComm = new SpatialFilter();
                arcSpatialFilter_rightIncComm.Geometry = outPoint_posRight;
                arcSpatialFilter_rightIncComm.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor arcIncCommCursor_right = muniFeatClass.Search(arcSpatialFilter_rightIncComm, false);
                IFeature arcFeatureIncComm_right = arcIncCommCursor_right.NextFeature();
                if (arcFeatureIncComm_right != null)
                {
                    spatialFieldValues.IncMuni_R =
                        arcFeatureIncComm_right.get_Value(arcFeatureIncComm_right.Fields.FindField("NAME")).ToString().Trim().ToUpper();
                }
                else
                {
                    spatialFieldValues.IncMuni_R = "";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcIncCommCursor_right);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_rightIncComm);


                // LEFT - COUNTY
                ISpatialFilter arcSpatialFilter_leftCounty = new SpatialFilter();
                arcSpatialFilter_leftCounty.Geometry = outPoint_negLeft;
                arcSpatialFilter_leftCounty.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor countyFeatureCursor_left = countyFeatClass.Search(arcSpatialFilter_leftCounty, false);
                IFeature arcFeatureCounty_left = countyFeatureCursor_left.NextFeature();
                if (arcFeatureCounty_left != null)
                {
                    spatialFieldValues.County_L =
                        arcFeatureCounty_left.get_Value(arcFeatureCounty_left.Fields.FindField("FIPS_STR")).ToString().Trim();
                }
                else
                {
                    spatialFieldValues.County_L = "NA";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(countyFeatureCursor_left);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_leftCounty);

                // RIGHT - COUNTY
                ISpatialFilter arcSpatialFilter_rightCounty = new SpatialFilter();
                arcSpatialFilter_rightCounty.Geometry = outPoint_posRight;
                arcSpatialFilter_rightCounty.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor countyFeatureCursor_right = countyFeatClass.Search(arcSpatialFilter_rightCounty, false);
                IFeature arcFeatureCounty_right = countyFeatureCursor_right.NextFeature();
                if (arcFeatureCounty_right != null)
                {
                    spatialFieldValues.County_R =
                        arcFeatureCounty_right.get_Value(arcFeatureCounty_right.Fields.FindField("FIPS_STR")).ToString().Trim();
                }
                else
                {
                    spatialFieldValues.County_R = "NA";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(countyFeatureCursor_right);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_rightCounty);


                // LEFT - ADDRESS SYSTEM
                ISpatialFilter arcSpatialFilter_leftAddrSystem = new SpatialFilter();
                arcSpatialFilter_leftAddrSystem.Geometry = outPoint_negLeft;
                arcSpatialFilter_leftAddrSystem.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor addrSystemFeatureCursor_left = addrSysFeatClass.Search(arcSpatialFilter_leftAddrSystem, false);
                IFeature arcFeatureAddrSystem_left = addrSystemFeatureCursor_left.NextFeature();
                if (arcFeatureAddrSystem_left != null)
                {
                    spatialFieldValues.AddrSystem_L =
                        arcFeatureAddrSystem_left.get_Value(arcFeatureAddrSystem_left.Fields.FindField("GRID_NAME")).ToString().Trim();
                    spatialFieldValues.AddrSystemQuad_L =
                        arcFeatureAddrSystem_left.get_Value(arcFeatureAddrSystem_left.Fields.FindField("QUADRANT")).ToString().Trim();
                }
                else
                {
                    spatialFieldValues.AddrSystem_L = "NA";
                    spatialFieldValues.AddrSystemQuad_L = "";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(addrSystemFeatureCursor_left);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_leftAddrSystem);

                // RIGHT - ADDRESS SYSTEM
                ISpatialFilter arcSpatialFilter_rightAddrSystem = new SpatialFilter();
                arcSpatialFilter_rightAddrSystem.Geometry = outPoint_posRight;
                arcSpatialFilter_rightAddrSystem.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                IFeatureCursor addrSystemFeatureCursor_right = addrSysFeatClass.Search(arcSpatialFilter_rightAddrSystem, false);
                IFeature arcFeatureAddrSystem_right = addrSystemFeatureCursor_right.NextFeature();
                if (arcFeatureAddrSystem_right != null)
                {
                    spatialFieldValues.AddrSystem_R =
                        arcFeatureAddrSystem_right.get_Value(arcFeatureAddrSystem_right.Fields.FindField("GRID_NAME")).ToString().Trim();
                    spatialFieldValues.AddrSystemQuad_R =
                        arcFeatureAddrSystem_right.get_Value(arcFeatureAddrSystem_right.Fields.FindField("QUADRANT")).ToString().Trim();
                }
                else
                {
                    spatialFieldValues.AddrSystem_R = "NA";
                    spatialFieldValues.AddrSystemQuad_R = "";
                }
                //clear out variables
                // release the cursor
                System.Runtime.InteropServices.Marshal.ReleaseComObject(addrSystemFeatureCursor_right);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_rightAddrSystem);


                // check if we're in Salt Lake County, if so check for metro townships
                if (utransFeature.get_Value(utransFeature.Fields.FindField("COFIPS")).ToString().Trim() == "49035")
                {
                    // it's in salt lake county - check for metro townships
                    // LEFT - METO TOWNSHIP
                    ISpatialFilter arcSpatialFilter_leftMetroTwnShp = new SpatialFilter();
                    arcSpatialFilter_leftMetroTwnShp.Geometry = outPoint_negLeft;
                    arcSpatialFilter_leftMetroTwnShp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                    IFeatureCursor metroTwnShpFeatureCursor_left = metroTwnShp.Search(arcSpatialFilter_leftMetroTwnShp, false);
                    IFeature arcFeatureMetroTwn_left = metroTwnShpFeatureCursor_left.NextFeature();
                    if (arcFeatureMetroTwn_left != null)
                    {
                        spatialFieldValues.UnIncMuni_L =
                            arcFeatureMetroTwn_left.get_Value(arcFeatureMetroTwn_left.Fields.FindField("SHORTDESC")).ToString().Trim().ToUpper();

                    }
                    else
                    {
                        spatialFieldValues.UnIncMuni_L = "";

                    }
                    //clear out variables
                    // release the cursor
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(metroTwnShpFeatureCursor_left);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_leftMetroTwnShp);

                    // RIGHT - METO TOWNSHIP
                    ISpatialFilter arcSpatialFilter_rightMetroTwnShp = new SpatialFilter();
                    arcSpatialFilter_rightMetroTwnShp.Geometry = outPoint_posRight;
                    arcSpatialFilter_rightMetroTwnShp.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                    IFeatureCursor metroTwnShpFeatureCursor_right = metroTwnShp.Search(arcSpatialFilter_rightMetroTwnShp, false);
                    IFeature arcFeatureMetroTwn_right = metroTwnShpFeatureCursor_right.NextFeature();
                    if (arcFeatureMetroTwn_right != null)
                    {
                        spatialFieldValues.UnIncMuni_R =
                            arcFeatureMetroTwn_right.get_Value(arcFeatureMetroTwn_right.Fields.FindField("SHORTDESC")).ToString().Trim().ToUpper();

                    }
                    else
                    {
                        spatialFieldValues.UnIncMuni_R = "";
                    }
                    //clear out variables
                    // release the cursor
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(metroTwnShpFeatureCursor_right);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(arcSpatialFilter_rightMetroTwnShp);
                }


                // destroy spatial filters.
                GC.Collect();

                // retrun object
                return spatialFieldValues;
            }
            catch (Exception)
            {
                throw;
            }


        }
    }
}
