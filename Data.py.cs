
using System;
using System.Diagnostics;
using System.Data.Common;

using System.Collections.Generic;
using ActiproSoftware.Windows.Shapes;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.ComponentModel;
using System.Threading.Channels;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Collections;
using ArcGIS.Core.CIM;
using System.Linq;
using System.Security.Policy;
using ArcGIS.Desktop.Internal.GeoProcessing;
using static ArcSWATPlus.LSUData;
using System.Windows.Documents;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Media;

namespace ArcSWATPlus {


    // Data collected about cells in watershed grid that make an HRU.
    public class CellData {

        public double area;

        public int cellCount;

        public int crop;

        public double totalElevation;

        public double totalSlope;

        public double totalLatitude;

        public double totalLongitude;

        public int actHRUNum;

        public CellData(int count, double area, double elevation, double slope, double x, double y, int crop) {
            //// Cell count
            this.cellCount = count;
            //// Total area in square metres
            this.area = area;
            // Total elevation (for calculating mean elevation)
            this.totalElevation = elevation;
            //// Total slope (for calculating mean slope)
            this.totalSlope = slope;
            // total latitude (in projected units) for calculating centroid
            this.totalLatitude = y;
            // total longitude (in projected units) for calculating centroid
            this.totalLongitude = x;
            //// Original crop number (for use with split landuses)
            this.crop = crop;
            // Actual HRU number (set when making HRUs, within printBasins)
            this.actHRUNum = 0;
        }

        // Add data for 1 cell.
        public void addCell(double area, double elevation, double slope, double x, double y) {
            this.cellCount += 1;
            this.area += area;
            this.totalElevation += elevation;
            this.totalSlope += slope;
            this.totalLatitude += y;
            this.totalLongitude += x;
        }

        // Add a cell data to this one.
        public void addCells(CellData cd) {
            this.cellCount += cd.cellCount;
            this.area += cd.area;
            this.totalElevation += cd.totalElevation;
            this.totalSlope += cd.totalSlope;
            this.totalLatitude += cd.totalLatitude;
            this.totalLongitude += cd.totalLongitude;
        }

        // Multiply cell values by factor.
        public void multiply(double factor) {
            this.cellCount = (Int32)Math.Round(this.cellCount * factor);
            this.area *= factor;
            this.totalSlope *= factor;
            this.totalElevation *= factor;
            this.totalLatitude *= factor;
            this.totalLongitude *= factor;
        }
    }

    // Roles a channel can play for a reservoir:
    // INNER means channel is part of reservoir: drainage to channel replaced by drainage to reservoir
    // OUTLET means channel is reservoir's outlet.  
    // OUTLET was intended to be used when multiple INNER channels drain into non-inner to avoid 
    // multiple separate reservoirs, but currently not implemented
    // INLET means a reservoir was located at the outlet end of a channel by the user, and cannot be merged downstream
    public enum ChannelRole : int {
        _INNER = 0,
        _OUTLET = 1,
        _INLET = 2
    }

    public enum WaterRole : int {
        _NONE = -1,
        _UNKNOWN = 0,
        _RESERVOIR = 1,
        _POND = 2,
        _WETLAND = 3,
        _PLAYA = 4
    }
    public class WaterBody {

        public int cellCount;
        public double area;
        public double originalArea;
        public double totalElevation;
        public double totalLatitude;
        public double totalLongitude;
        public int id;
        public ChannelRole channelRole;
        public WaterRole waterRole;

        //Data about areas with landuse WATR
        public WaterBody(int count, double area, double elevation, double x, double y) {
            //Constructor
            // Cell count
            this.cellCount = count;
            // Total area in square metres.  
            // downstream area is increased when waterbodies on adjacent channels are merged
            this.area = area;
            // Original area before any merging
            this.originalArea = area;
            // Total elevation (for calculating mean elevation)
            this.totalElevation = elevation;
            // total latitude (in projected units) for calculating centroid
            this.totalLatitude = y;
            // total longitude (in projected units) for calculating centroid
            this.totalLongitude = x;
            // id set later
            this.id = 0;
            // role of channel: _INNER is default
            this.channelRole = ChannelRole._INNER;
            // water role
            this.waterRole = WaterRole._UNKNOWN;
        }


        public void addCell(double area, double elevation, double x, double y) {
            //Add data for 1 cell
            this.cellCount += 1;
            this.area += area;
            this.originalArea += area;
            this.totalElevation += elevation;
            this.totalLatitude += y;
            this.totalLongitude += x;
        }

        // public void addCells(CellData cd) {
        //         //Add an hru to this one
        // this.cellCount += cd.cellCount
        // this.area += cd.area
        // this.originalArea += cd.area
        // this.totalElevation += cd.totalElevation
        // this.totalLatitude += cd.totalLatitude
        // this.totalLongitude += cd.totalLongitude

        public void addWater(WaterBody wb, bool keepOriginal) {
            //Add water body to this one, keeping original area, if keepOriginal is true
            this.cellCount += wb.cellCount;
            this.area += wb.area;
            if (!keepOriginal) {
                this.originalArea += wb.originalArea;
            }
            this.totalElevation += wb.totalElevation;
            this.totalLatitude += wb.totalLatitude;
            this.totalLongitude += wb.totalLongitude;
        }

        public void multiply(double factor) {
            //Multiply water body values by factor
            this.cellCount = Convert.ToInt32(this.cellCount * factor + 0.5);
            this.area *= factor;
            this.originalArea *= factor;
            this.totalElevation *= factor;
            this.totalLatitude *= factor;
            this.totalLongitude *= factor;
        }

        public void setInlet() {
            //Set  to inlet.  provents merging downstream
            this.channelRole = ChannelRole._INLET;
        }


        public void setOutlet() {
            //Set  to outlet
            this.channelRole = ChannelRole._OUTLET;
        }


        public void setReservoir() {
            //Set to reservoir
            this.waterRole = WaterRole._RESERVOIR;
        }


        public void setPond() {
            //Set to pond
            this.waterRole = WaterRole._POND;
        }


        public bool isInlet() {
            //Return true if  is inlet
            return this.channelRole == ChannelRole._INLET;
        }

        public bool isOutlet() {
            //Return true if  is outlet
            return this.channelRole == ChannelRole._OUTLET;
        }

        public bool isUnknown() {
            //Return true if water role is unknown
            return this.waterRole == WaterRole._UNKNOWN;
        }

        public bool isReservoir() {
            //Return true if is reservoir
            return this.waterRole == WaterRole._RESERVOIR;
        }

        public bool isPond() {
            //Return true if is pond
            return this.waterRole == WaterRole._POND;
        }

        public WaterBody copy() {

            WaterBody result = new WaterBody(this.cellCount, this.area, this.totalElevation, this.totalLongitude, this.totalLatitude);
            result.originalArea = this.originalArea;
            result.channelRole = this.channelRole;
            result.waterRole = this.waterRole;
            result.id = this.id;
            return result;
        }
    }

    public class LSUData {

        public int cellCount;
        public double area;
        public double outletElevation;
        public double sourceElevation;
        public double channelLength;
        public int channelOrder;
        public double farElevation;
        public double farDistance;
        public double farPointX;
        public double farPointY;
        public double midPointX;
        public double midPointY;
        public double totalElevation;
        public double totalSlope;
        public double totalLatitude;
        public double totalLongitude;
        public double cropSoilSlopeArea;
        public Dictionary<int, CellData> hruMap;
        public Dictionary<int, Dictionary<int, Dictionary<int, int>>> cropSoilSlopeNumbers;
        public Dictionary<int, double> cropAreas;
        public Dictionary<int, double> originalCropAreas;
        public Dictionary<int, double> soilAreas;
        public Dictionary<int, double> originalSoilAreas;
        public Dictionary<int, double> slopeAreas;
        public Dictionary<int, double> originalSlopeAreas;
        public WaterBody waterBody;
        public int lastHru;

        //Data held about landscape unit.

        //Note that pixels of landuse WATR appear in both the main LSU data and also in the water body component.
        //A decision is made later according to the water role:
        //if the water role is a reservoir or pond then the water body is added to the model as reservoir or pond, and WATR HRUs are removed.
        //If the water role is _UNKNOWN then the water body is ignored and the WATR HRUs included. 
        //
        public LSUData() {
            //Initialise class variables
            // Number of cells
            this.cellCount = 0;
            // Area in square metres
            this.area = 0;
            // elevation at outlet
            this.outletElevation = 0;
            // source of channel elevation
            this.sourceElevation = 0;
            // channel length in m
            this.channelLength = 0;
            // Strahler order
            this.channelOrder = 0;
            // elevation of point with longest flow distance 
            this.farElevation = 0;
            // longest flow distance to channel
            // set negative so even a zero distance will be recorded
            this.farDistance = -1;
            // longitude (in projected units) of point with longest flow distance
            this.farPointX = 0;
            // latitude (in projected units) of point with longest flow distance
            this.farPointY = 0;
            // longitude (in projected units) of channel's mid point
            this.midPointX = 0;
            // latitude (in projected units) of channel's mid point
            this.midPointY = 0;
            // Total of elevation values (to compute mean)
            this.totalElevation = 0;
            // total of latitudes (in projected units) (to compute centroid)
            this.totalLatitude = 0;
            // total of longitudes (in projected units) (to compute centroid)
            this.totalLongitude = 0;
            // Total of slope values (to compute mean)
            this.totalSlope = 0;
            // area in square metres with not-Nodata crop, soil, and slope values.
            this.cropSoilSlopeArea = 0;
            // Map hru (relative) number -> CellData.
            this.hruMap = new Dictionary<int, CellData>();
            // Nested map crop -> soil -> slope -> hru number.
            // Range of cropSoilSlopeNumbers must be same as domain of hruMap
            this.cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            // Map of crop to area of crop in landscape unit.
            //
            // This and the similar maps for soil and slope are duplicated:
            // an original version created after basin data is calculated and 
            // before HRUs are created, and another after HRUs are created.
            // HRU creation occurs within landscape units if these are used,
            // so landscape areas do not change when HRUs are created.
            this.cropAreas = new Dictionary<int, double>();
            // Original crop area map
            this.originalCropAreas = new Dictionary<int, double>();
            // Map of soil to area of soil in landscape unit.
            this.soilAreas = new Dictionary<int, double>();
            // Original soil area map
            this.originalSoilAreas = new Dictionary<int, double>();
            // Map of slope to area of slope in landscape unit.
            this.slopeAreas = new Dictionary<int, double>();
            // Original slope area map
            this.originalSlopeAreas = new Dictionary<int, double>();
            // water body if any
            this.waterBody = null;
            // last HRU number used
            this.lastHru = 0;
        }

        public Dictionary<int, double> cropSoilAreas(int crop) {
            // Map of soil -> area in square metres for particular crop

            //         assert crop in this.cropSoilSlopeNumbers
            var csmap = new Dictionary<int, double>();
            foreach (var soil in this.cropSoilSlopeNumbers[crop].Keys) {
                csmap[soil] = this.cropSoilArea(crop, soil);
            }
            return csmap;
        }

        public double cropSoilArea(int crop, int soil) {
            // Area in square metres for crop-soil combination

            double area = 0.0;
            //         assert crop in this.cropSoilSlopeNumbers and soil in this.cropSoilSlopeNumbers[crop]
            var slopeNumbers = this.cropSoilSlopeNumbers[crop][soil];
            foreach (int hru in slopeNumbers.Values) {
                var cellData = this.hruMap[hru];
                area += cellData.area;
            }
            return area;
        }

        public double cropArea(int crop) {
            // Area in square metres for crop

            double area = 0.0;
            // use when cropAreas may not be set
            //         assert crop in this.cropSoilSlopeNumbers, u'Landuse {0} not in basin data', crop)
            foreach (var slopeNumbers in this.cropSoilSlopeNumbers[crop].Values) {
                foreach (int hru in slopeNumbers.Values) {
                    var cellData = this.hruMap[hru];
                    area += cellData.area;
                }
            }
            return area;
        }

        public Dictionary<int, double> cropSoilSlopeAreas(int crop, int soil) {
            // Map of slope -> area in square metres for this crop and soil
            var cssmap = new Dictionary<int, double>();

            //         assert crop in this.cropSoilSlopeNumbers and soil in this.cropSoilSlopeNumbers[crop]
            foreach (var (slope, hru) in this.cropSoilSlopeNumbers[crop][soil]) {
                cssmap[slope] = this.hruMap[hru].area;
            }
            return cssmap;
        }

        public (int, int, int) getDominantHRU(int waterLanduse, bool allowWater) {
            // Find the HRU with the largest area, 
            // and return its crop, soil and slope.  
            // If allowWater is False, waterLanduse crop is ignored.
            // 
            double maxArea = 0.0;
            int maxCrop = -1;
            int maxSoil = -1;
            int maxSlope = -1;

            foreach (var (crop, soilSlopeNumbers) in this.cropSoilSlopeNumbers) {
                if (crop != waterLanduse || allowWater) {
                    foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                        foreach (var (slope, hru) in slopeNumbers) {
                            var cellData = this.hruMap[hru];
                            var area = cellData.area;
                            if (area > maxArea) { }
                            maxArea = area;
                            maxCrop = crop;
                            maxSoil = soil;
                            maxSlope = slope;
                        }
                    }
                }
            }
            return (maxCrop, maxSoil, maxSlope);
        }

        public void redistribute(double factor) {
            // Multiply all the HRU areas by factor

            foreach (var (hru, cellData) in this.hruMap) {
                cellData.multiply(factor);
                this.hruMap[hru] = cellData;
            }
        }

        public void moveWaterToPond(double lakeArea, int waterLanduse) {
            // If the lsu has a lake pond and a waterBody that is currently unknown(not otherwise marked as pond or reservoir)
            // then water up to the area of the lake is removed from water body and water HRUs and the crop HRUs increased to compensate

            double cropRedistributeFactor;
            double waterRedistributeFactor;
            var waterHRUs = new List<int>();
            var cropHRUs = new List<int>();

            // ensure there are crop HRUs to be enlarged
            if (this.waterBody is not null && this.waterBody.isUnknown() && this.cropSoilSlopeArea > this.waterBody.area) {
                if (this.waterBody.area <= lakeArea) {
                    cropRedistributeFactor = this.cropSoilSlopeArea / (this.cropSoilSlopeArea - this.waterBody.area);
                    this.removeWaterHRUs(waterLanduse);
                    this.waterBody = null;
                    waterRedistributeFactor = 0;  // will not be used since now no water HRUs, but should be defined
                } else {
                    waterRedistributeFactor = (this.waterBody.area - lakeArea) / this.waterBody.area;
                    this.waterBody.multiply(waterRedistributeFactor);
                    cropRedistributeFactor = (this.cropSoilSlopeArea - this.waterBody.area + lakeArea) / (this.cropSoilSlopeArea - this.waterBody.area);
                }
                foreach (var (crop, soilSlopeNumbers) in this.cropSoilSlopeNumbers) {
                    foreach (var slopeNumbers in soilSlopeNumbers.Values) {
                        foreach (var hruNum in slopeNumbers.Values) {
                            if (crop == waterLanduse) {
                                waterHRUs.Add(hruNum);
                            } else {
                                cropHRUs.Add(hruNum);
                            }
                            foreach (var hru in waterHRUs) {
                                this.hruMap[hru].multiply(waterRedistributeFactor);
                            }
                            foreach (var hru in cropHRUs) {
                                this.hruMap[hru].multiply(cropRedistributeFactor);
                            }
                        }
                    }
                }
            }
        }

        public void redistributeNodataAndWater(int chLink, int lscape, IEnumerable<int> chLinksByLakes, int waterLanduse) {
            //Add nodata areas proportionately to originalareas. 
            //Also removes water body if not a separate reservoir, pond or wetland
            //and channel flows into or is inside lake and lscape is nolandscape or floodplain


            // water bodies adjacent to lakes are counted as part of lakes, and whole lsu area is counted as land 
            // but beware making area zero if all is water
            if (this.waterBody is not null && this.cropSoilSlopeArea > this.waterBody.originalArea &&
                this.waterBody.isUnknown() && chLinksByLakes.Contains(chLink) &&
                (lscape == Utils._NOLANDSCAPE || lscape == Utils._FLOODPLAIN)) {
                this.removeWaterHRUs(waterLanduse);
                this.waterBody = null;
            }
            double areaToRedistribute = this.area - this.cropSoilSlopeArea;
            if (this.area > areaToRedistribute && areaToRedistribute > 0) {
                double redistributeFactor = this.area / (this.area - areaToRedistribute);
                this.redistribute(redistributeFactor);
            }
        }


        public void removeHRU(int hru, int crop, int soil, int slope) {
            // Remove an HRU from the hruMap and the cropSoilSlopeNumbers map
            //         assert crop in this.cropSoilSlopeNumbers and \
            //             soil in this.cropSoilSlopeNumbers[crop] and \
            //             slope in this.cropSoilSlopeNumbers[crop][soil] and \
            //             hru == this.cropSoilSlopeNumbers[crop][soil][slope]
            this.hruMap.Remove(hru);
            this.cropSoilSlopeNumbers[crop][soil].Remove(slope);
            if (this.cropSoilSlopeNumbers[crop][soil].Count == 0) {
                this.cropSoilSlopeNumbers[crop].Remove(soil);
                if (this.cropSoilSlopeNumbers[crop].Count == 0) {
                    this.cropSoilSlopeNumbers.Remove(crop);
                }
            }
        }

        // Note unlike removeHRU it changes the cropSoilSlopeArea            
        public void removeWaterHRUs(int waterLanduse) {
            //Remove HRUs with landuse water//

            // note use of lists as dictionaries changed within loop
            foreach (var (crop, soilSlopeNumbers) in this.cropSoilSlopeNumbers.ToList()) {
                if (crop == waterLanduse) {
                    foreach (var (soil, slopeNumbers) in soilSlopeNumbers.ToList()) {
                        foreach (var (slope, hru) in slopeNumbers.ToList()) {
                            this.cropSoilSlopeArea -= this.hruMap[hru].area;
                            this.removeHRU(hru, crop, soil, slope);
                        }
                    }
                }
            }
        }

        public int totalHRUCellCount() {
            //Total cell count of HRUs in this LSU

            int totalCellCount = 0;
            foreach (var hruData in this.hruMap.Values) {
                totalCellCount += hruData.cellCount;
            }
            return totalCellCount;
        }

        public double totalHRUSlopes() {
            //Total slope values of HRUs in this LSU

            double totalSlope = 0;
            foreach (var hruData in this.hruMap.Values) {
                totalSlope += hruData.totalSlope;
            }
            return totalSlope;
        }

        public void setCropAreas(bool isOriginal) {
            // Make map crop -> area from hruMap and cropSoilSlopeNumbers

            var cmap = isOriginal ? this.originalCropAreas : this.cropAreas;
            cmap.Clear();
            foreach (var (crop, soilSlopeNumbers) in this.cropSoilSlopeNumbers) {
                var area = 0.0;
                foreach (var slopeNumbers in soilSlopeNumbers.Values) {
                    foreach (var hru in slopeNumbers.Values) {
                        var cellData = this.hruMap[hru];
                        area += cellData.area;
                    }
                }
                cmap[crop] = area;
            }
        }

        public void setSoilAreas(bool isOriginal) {
            // Make map soil -> area from hruMap and cropSoilSlopeNumbers

            var smap = isOriginal ? this.originalSoilAreas : this.soilAreas;
            smap.Clear();
            foreach (var soilSlopeNumbers in this.cropSoilSlopeNumbers.Values) {
                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                    foreach (var hru in slopeNumbers.Values) {
                        var cellData = this.hruMap[hru];
                        var area = 0.0;
                        if (!smap.TryGetValue(soil, out area)) {
                            area = 0.0;
                        }
                        smap[soil] = area + cellData.area;
                    }
                }
            }
        }

        public void setSlopeAreas(bool isOriginal) {
            // Make map slope -> area from hruMap and cropSoilSlopeNumbers
            var smap = isOriginal ? this.originalSlopeAreas : this.slopeAreas;
            smap.Clear();
            foreach (var soilSlopeNumbers in this.cropSoilSlopeNumbers.Values) {
                foreach (var slopeNumbers in soilSlopeNumbers.Values) {
                    foreach (var (slope, hru) in slopeNumbers) {
                        var cellData = this.hruMap[hru];
                        var area = 0.0;
                        if (!smap.TryGetValue(slope, out area)) {
                            area = 0.0;
                        }
                        smap[slope] = area + cellData.area;
                    }
                }
            }
        }

        public int nextHruNumber() {
            //Return a new HRU number for use, incrementing last HRU number used

            this.lastHru += 1;
            return this.lastHru;
        }

        public int getHruNumber(int crop, int soil, int slope) {
            //
            //Return HRU number (new if necessary, adding one to last hru used for the landscape unit) 
            //for the crop/soil/slope combination.

            Dictionary<int, Dictionary<int, int>> soilSlopeNumbers = null;
            if (!this.cropSoilSlopeNumbers.TryGetValue(crop, out soilSlopeNumbers)) {
                // new crop
                this.lastHru += 1;
                var slopeNumbers = new Dictionary<int, int>();
                slopeNumbers[slope] = this.lastHru;
                soilSlopeNumbers = new Dictionary<int, Dictionary<int, int>>();
                soilSlopeNumbers[soil] = slopeNumbers;
                this.cropSoilSlopeNumbers[crop] = soilSlopeNumbers;
            } else {
                Dictionary<int, int> slopeNumbers = null;
                if (!soilSlopeNumbers.TryGetValue(soil, out slopeNumbers)) {
                    // new soil for existing crop
                    this.lastHru += 1;
                    slopeNumbers = new Dictionary<int, int>();
                    slopeNumbers[slope] = this.lastHru;
                    soilSlopeNumbers[soil] = slopeNumbers;
                } else {
                    int hru = -1;
                    if (!slopeNumbers.TryGetValue(slope, out hru)) {
                        // new slope for existing crop and soil
                        this.lastHru += 1;
                        slopeNumbers[slope] = this.lastHru;
                    } else {
                        return hru;
                    }
                }
            }
            return this.lastHru;
        }

        public LSUData copy() {
            //Return deep copy of self

            LSUData result = new LSUData();
            result.cellCount = this.cellCount;
            result.area = this.area;
            result.outletElevation = this.outletElevation;
            result.sourceElevation = this.sourceElevation;
            result.channelLength = this.channelLength;
            result.channelOrder = this.channelOrder;
            result.farElevation = this.farElevation;
            result.farDistance = this.farDistance;
            result.farPointX = this.farPointX;
            result.farPointY = this.farPointY;
            result.midPointX = this.midPointX;
            result.midPointY = this.midPointY;
            result.totalElevation = this.totalElevation;
            result.totalSlope = this.totalSlope;
            result.totalLatitude = this.totalLatitude;
            result.totalLongitude = this.totalLongitude;
            result.cropSoilSlopeArea = this.cropSoilSlopeArea;
            result.hruMap = this.copyHRUMap();
            result.cropSoilSlopeNumbers = this.copyCropSoilSlopeNumbers();
            result.cropAreas = LSUData.copyAreaMap(this.cropAreas);
            result.originalCropAreas = LSUData.copyAreaMap(this.originalCropAreas);
            result.soilAreas = LSUData.copyAreaMap(this.soilAreas);
            result.originalSoilAreas = LSUData.copyAreaMap(this.originalSoilAreas);
            result.slopeAreas = LSUData.copyAreaMap(this.slopeAreas);
            result.originalSlopeAreas = LSUData.copyAreaMap(this.originalSlopeAreas);
            if (this.waterBody is not null) {
                result.waterBody = this.waterBody.copy();
            }
            result.lastHru = this.lastHru;
            return result;
        }

        public Dictionary<int, CellData> copyHRUMap() {
            //Return deep copy of hruMap

            var result = new Dictionary<int, CellData>();

            foreach (var (hru, cellData) in this.hruMap) {
                var cd = new CellData(cellData.cellCount, cellData.area, cellData.totalElevation,
                                       cellData.totalSlope, cellData.totalLongitude, cellData.totalLatitude, cellData.crop);
                cd.actHRUNum = cellData.actHRUNum;
                result[hru] = cd;
            }
            return result;
        }

        public Dictionary<int, Dictionary<int, Dictionary<int, int>>> copyCropSoilSlopeNumbers() {
            //Return deep copy of cropSoilSlopeNumbers

            var result = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
            foreach (var (crop, soilSlopeNumbers) in this.cropSoilSlopeNumbers) {
                result[crop] = new Dictionary<int, Dictionary<int, int>>();
                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                    result[crop][soil] = new Dictionary<int, int>();
                    foreach (var (slope, hru) in slopeNumbers) {
                        result[crop][soil][slope] = hru;
                    }
                }
            }
            return result;
        }

        public static Dictionary<int, double> copyAreaMap(Dictionary<int, double> amap) {
            //Return deep copy of int -> double amap

            var result = new Dictionary<int, double>();

            foreach (var (key, val) in amap) {
                result[key] = val;
            }
            return result;
        }

        public void merge(LSUData lsuData) {
            //Merge lsuData into this data

            this.cellCount += lsuData.cellCount;
            this.area += lsuData.area;
            // merge is always down, i.e. LSUData is being merged downstream into self
            this.sourceElevation = lsuData.sourceElevation;
            if (this.farDistance < lsuData.farDistance + this.channelLength) {
                this.farElevation = lsuData.farElevation;
                this.farPointX = lsuData.farPointX;
                this.farPointY = lsuData.farPointY;
                this.farDistance = lsuData.farDistance + this.channelLength;
            }
            this.midPointX = (this.midPointX + lsuData.midPointX) / 2;
            this.midPointY = (this.midPointY + lsuData.midPointY) / 2;
            this.channelLength += lsuData.channelLength;
            if (this.channelOrder < lsuData.channelOrder) {
                this.channelOrder = lsuData.channelOrder;
            }
            this.totalElevation += lsuData.totalElevation;
            this.totalSlope += lsuData.totalSlope;
            this.totalLatitude += lsuData.totalLatitude;
            this.totalLongitude += lsuData.totalLongitude;
            this.cropSoilSlopeArea += lsuData.cropSoilSlopeArea;
            foreach (var (crop, soilSlopeNumbers) in lsuData.cropSoilSlopeNumbers) {
                if (!this.cropSoilSlopeNumbers.ContainsKey(crop)) {
                    this.cropSoilSlopeNumbers.Add(crop, new Dictionary<int, Dictionary<int, int>>());
                }
                var mySoilSlopeNumbers = this.cropSoilSlopeNumbers[crop];
                foreach (var (soil, slopeNumbers) in soilSlopeNumbers) {
                    if (!mySoilSlopeNumbers.ContainsKey(soil)) {
                        mySoilSlopeNumbers.Add(soil, new Dictionary<int, int>());
                    }
                    var mySlopeNumbers = mySoilSlopeNumbers[soil];
                    foreach (var (slope, hru) in slopeNumbers) {
                        int myHru = -1;
                        if (!mySlopeNumbers.TryGetValue(slope, out myHru)) {
                            this.lastHru += 1;
                            mySlopeNumbers[slope] = this.lastHru;
                            this.hruMap[this.lastHru] = lsuData.hruMap[hru];
                        } else {
                            this.hruMap[myHru].addCells(lsuData.hruMap[hru]);
                        }
                    }
                }
            }
            LSUData.mergeMaps(this.cropAreas, lsuData.cropAreas);
            LSUData.mergeMaps(this.originalCropAreas, lsuData.originalCropAreas);
            LSUData.mergeMaps(this.soilAreas, lsuData.soilAreas);
            LSUData.mergeMaps(this.originalSoilAreas, lsuData.originalSoilAreas);
            LSUData.mergeMaps(this.slopeAreas, lsuData.slopeAreas);
            LSUData.mergeMaps(this.originalSlopeAreas, lsuData.originalSlopeAreas);
            if (this.waterBody is null) {
                this.waterBody = lsuData.waterBody;
            } else {
                if (lsuData.waterBody is not null) {
                    this.waterBody.addWater(lsuData.waterBody, false);
                }
            }
        }

        public void makeReservoir(double threshold) {
            //Set water body to a reservoir if water area exceeds threshold percent of LSU

            if (this.waterBody is not null && this.waterBody.area > threshold * this.area / 100) {
                this.waterBody.waterRole = WaterRole._RESERVOIR;
            }
        }

        public static void mergeMaps(Dictionary<int, double> map1, Dictionary<int, double> map2) {
            //Add data from map2 to map1.

            foreach (var (key, val) in map2) {
                if (!map1.ContainsKey(key)) {
                    map1[key] = val;
                } else {
                    map1[key] += val;
                }
            }
        }
    }

    // Data held about subbasin.
    public class BasinData {

        public Dictionary<int, Dictionary<int, LSUData>> lsus;

        public Dictionary<int, Dictionary<int, LSUData>> mergedLsus;

        public double farDistance;

        public double minElevation;

        public double maxElevation;

        public int waterLanduse;

        public int waterId;

        public BasinData(int waterLanduse, double farDistance, int waterId) {
            //Initialise class variables.
            // Map channel -> landscape category -> LSUData
            this.lsus = new Dictionary<int, Dictionary<int, LSUData>>();
            // results of doing merges
            this.mergedLsus = null;
            // Elevation in metres of lowest point in the subbasin
            this.minElevation = 8849; // everest plus 1
                                      // Elevation in metres of highest point in the subbasin
            this.maxElevation = -419; // dead sea minimum minus 1
                                      // longest flow length to subbasin outlet
                                      // initialised for existing non-grid watershed to maximum channel flow length
                                      // otherwise to -1 so later values passed by addCell will update it
            this.farDistance = farDistance;
            // landuse (crop) value for WATR
            this.waterLanduse = waterLanduse;
            // waterId used with grid models to indicate cells that are part of lakes but need to be recorded in gis_subbasins
            this.waterId = waterId;
        }

        public Dictionary<int, Dictionary<int, LSUData>> getLsus() {
            // Return mergedLsus if it exists, else lsus.
            return this.mergedLsus is null ? this.lsus : this.mergedLsus;
        }

        // Add data for 1 cell in watershed raster.
        public void addCell(
            int channel,
            int landscape,
            int crop,
            int soil,
            int slope,
            double area,
            double elevation,
            double slopeValue,
            double distSt,
            double distCh,
            double x,
            double y,
            GlobalVars _gv) {

            Dictionary<int, LSUData> channelData = null;
            if (!this.lsus.TryGetValue(channel, out channelData)) {
                this.lsus[channel] = new Dictionary<int, LSUData>();
            }
            channelData = this.lsus[channel];
            LSUData lsuData;
            if (channelData.ContainsKey(landscape)) {
                lsuData = channelData[landscape];
            } else {
                lsuData = new LSUData();
                var reachData = _gv.topo.channelsData[channel];
                lsuData.outletElevation = reachData.lowerZ;
                lsuData.sourceElevation = reachData.upperZ;

                // mid point of channel is a notional point midway between ends
                lsuData.midPointX = (reachData.lowerX + reachData.upperX) / 2;
                lsuData.midPointY = (reachData.lowerY + reachData.upperY) / 2;
                lsuData.channelLength = _gv.topo.channelLengths[channel];
                lsuData.channelOrder = _gv.topo.strahler[channel];
                if (_gv.existingWshed) {
                    lsuData.farDistance = lsuData.channelLength;
                    lsuData.farElevation = lsuData.sourceElevation;
                }
                channelData[landscape] = lsuData;
            }
            lsuData.cellCount += 1;
            lsuData.area += area;
            lsuData.totalLatitude += y;
            lsuData.totalLongitude += x;
            if (slopeValue != _gv.slopeNoData) {
                lsuData.totalSlope += slopeValue;
            }
            if (!Utils.areClose(elevation, _gv.elevationNoData, 1E-06)) {
                lsuData.totalElevation += elevation;
            }
            if (distSt != _gv.distStNoData && distSt > this.farDistance) {
                // We have found a new  farthest (by flow distance) point from the subbasin outlet:
                // store distance
                this.farDistance = distSt;
            }
            if (distCh != _gv.distChNoData && distCh > lsuData.farDistance) {
                // We have found a new farthest (by flow distance) point from the channel:
                // store distance, its elevation and its location
                lsuData.farDistance = distCh;
                lsuData.farElevation = elevation;
                lsuData.farPointX = x;
                lsuData.farPointY = y;
            }
            this.maxElevation = Math.Max(this.maxElevation, elevation);
            this.minElevation = Math.Min(this.minElevation, elevation);
            if (crop == this.waterLanduse) {
                if (lsuData.waterBody is null) {
                    lsuData.waterBody = new WaterBody(1, area, elevation, x, y);
                } else {
                    lsuData.waterBody.addCell(area, elevation, x, y);
                }
            }
            if ((crop != _gv.cropNoData) && (soil != _gv.soilNoData) && (slopeValue != _gv.slopeNoData)) {
                lsuData.cropSoilSlopeArea += area;
                var hru = lsuData.getHruNumber(crop, soil, slope);
                CellData cellData = null;
                if (!lsuData.hruMap.TryGetValue(hru, out cellData)) {
                    // new hru
                    cellData = new CellData(1, area, elevation, slopeValue, x, y, crop);
                    lsuData.hruMap[hru] = cellData;
                } else {
                    cellData.addCell(area, elevation, slopeValue, x, y);
                    lsuData.hruMap[hru] = cellData;
                }
            }
        }

        // Return HRU number (new if necessary, adding one to input hru number) 
        //         for the crop/soil/slope combination.
        //         
        public static int getHruNumber(
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>> channelLandscapeCropSoilSlopeNumbers,
            int lastHru,
            int channel,
            int landscape,
            int crop,
            int soil,
            int slope) {

            int hru;
            Dictionary<int, int> slopeNumbers;
            Dictionary<int, Dictionary<int, int>> soilSlopeNumbers;
            Dictionary<int, Dictionary<int, Dictionary<int, int>>> cropSoilSlopeNumbers;
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>> landscapeCropSoilSlopeNumbers;
            var resultHru = lastHru;
            if (!channelLandscapeCropSoilSlopeNumbers.TryGetValue(channel, out landscapeCropSoilSlopeNumbers)) {
                //new channel
                resultHru += 1;
                slopeNumbers = new Dictionary<int, int>();
                slopeNumbers[slope] = resultHru;
                soilSlopeNumbers = new Dictionary<int, Dictionary<int, int>>();
                soilSlopeNumbers[soil] = slopeNumbers;
                cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                cropSoilSlopeNumbers[crop] = soilSlopeNumbers;
                landscapeCropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, int>>>>();
                landscapeCropSoilSlopeNumbers[landscape] = cropSoilSlopeNumbers;
                channelLandscapeCropSoilSlopeNumbers[channel] = landscapeCropSoilSlopeNumbers;
            } else {
                if (!landscapeCropSoilSlopeNumbers.TryGetValue(landscape, out cropSoilSlopeNumbers)) {
                    //new landscape 
                    resultHru += 1;
                    slopeNumbers = new Dictionary<int, int>();
                    slopeNumbers[slope] = resultHru;
                    soilSlopeNumbers = new Dictionary<int, Dictionary<int, int>>();
                    soilSlopeNumbers[soil] = slopeNumbers;
                    cropSoilSlopeNumbers = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();
                    cropSoilSlopeNumbers[crop] = soilSlopeNumbers;
                    landscapeCropSoilSlopeNumbers[landscape] = cropSoilSlopeNumbers;
                } else {
                    if (!cropSoilSlopeNumbers.TryGetValue(crop, out soilSlopeNumbers)) {
                        // new crop
                        resultHru += 1;
                        slopeNumbers = new Dictionary<int, int>();
                        slopeNumbers[slope] = resultHru;
                        soilSlopeNumbers = new Dictionary<int, Dictionary<int, int>>();
                        soilSlopeNumbers[soil] = slopeNumbers;
                        cropSoilSlopeNumbers[crop] = soilSlopeNumbers;
                    } else {
                        if (!soilSlopeNumbers.TryGetValue(soil, out slopeNumbers)) {
                            // new soil for existing crop
                            resultHru += 1;
                            slopeNumbers = new Dictionary<int, int>();
                            slopeNumbers[slope] = resultHru;
                            soilSlopeNumbers[soil] = slopeNumbers;
                        } else {
                            if (!slopeNumbers.TryGetValue(slope, out hru)) {
                                // new slope for existing crop and soil
                                resultHru += 1;
                                slopeNumbers[slope] = resultHru;
                            } else {
                                return hru;
                            }
                        }
                    }
                }
            }
            return resultHru;
        }

        public void merge(int channel, int target) {
            // Merge channel into target.  Assumed both channels initially in domain of mergedLsus.  Finally only target is."

            var channelData = this.mergedLsus[channel];
            var targetData = this.mergedLsus[target];
            foreach (var (landscape, lsuData) in channelData) {
                if (targetData.ContainsKey(landscape)) {
                    targetData[landscape].merge(lsuData);
                } else {
                    targetData[landscape] = lsuData;
                }
            }
            this.mergedLsus.Remove(channel);
        }

        // Set area maps for crop, soil and slope.
        //         Add nodata area to HRUs if redistributeNodata, else reduce basin cellCount, area and totalSlope to total of defined HRUs.
        public void setAreas(bool isOriginal, IEnumerable<int> chLinksByLakes, int waterLanduse, bool redistributeNodata = true) {

            foreach (var (chLink, channelData) in this.getLsus()) {
                foreach (var (lscape, lsuData) in channelData) {
                    if (isOriginal) {
                        if (redistributeNodata) {
                            // nodata area is included in final areas: need to add to original
                            // so final and original tally
                            // also remove water bodies that should be incorporated into lakes
                            lsuData.redistributeNodataAndWater(chLink, lscape, chLinksByLakes, waterLanduse);
                        } else {
                            // if we are not redistributing nodata, need to correct the basin area, cell count and totalSlope, which may be reduced 
                            // as we are removing nodata area from the model
                            var waterArea = lsuData.waterBody is null ? 0 : lsuData.waterBody.area;
                            lsuData.area = lsuData.cropSoilSlopeArea + waterArea;
                            lsuData.cellCount = lsuData.totalHRUCellCount();
                            lsuData.totalSlope = lsuData.totalHRUSlopes();
                        }
                    }
                    lsuData.setCropAreas(isOriginal);
                    lsuData.setSoilAreas(isOriginal);
                    lsuData.setSlopeAreas(isOriginal);
                }
            }
        }

        public Dictionary<int, double> cropAreas(bool isOriginal) {
            //Return map of crop to area in square metres across subbasin, before HRU calculation if isOriginal

            var result = new Dictionary<int, double>();

            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    var cropAreas = isOriginal ? lsuData.originalCropAreas : lsuData.cropAreas;
                    foreach (var (crop, area) in cropAreas) {

                        double cropArea;
                        if (result.TryGetValue(crop, out cropArea)) {
                            result[crop] = cropArea + area;
                        }
                    }
                }
            }
            return result;
        }

        public Dictionary<int, double> soilAreas(bool isOriginal) {
            //Return map of soil to area in square metres across subbasin, before HRU calculation if isOriginal

            var result = new Dictionary<int, double>();

            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    var soilAreas = isOriginal ? lsuData.originalSoilAreas : lsuData.soilAreas;
                    foreach (var (soil, area) in soilAreas) {

                        double soilArea;
                        if (result.TryGetValue(soil, out soilArea)) {
                            result[soil] = soilArea + area;
                        }
                    }
                }
            }
            return result;
        }

        public Dictionary<int, double> slopeAreas(bool isOriginal) {
            //Return map of slope to area in square metres across subbasin, before HRU calculation if isOriginal

            var result = new Dictionary<int, double>();

            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    var slopeAreas = isOriginal ? lsuData.originalSlopeAreas : lsuData.slopeAreas;
                    foreach (var (slope, area) in slopeAreas) {

                        double slopeArea;
                        if (result.TryGetValue(slope, out slopeArea)) {
                            result[slope] = slopeArea + area;
                        }
                    }
                }
            }
            return result;
        }

        // Total cell count of all landscape units in the subbasin.
        public virtual int subbasinCellCount() {
            var totalCellCount = 0;
            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    totalCellCount += lsuData.cellCount;
                }
            }
            return totalCellCount;
        }

        // Total area in square metres of the subbasin (land and water).
        public virtual double subbasinArea() {
            var totalArea = 0.0;
            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    totalArea += lsuData.area;
                }
            }
            return totalArea;
        }

        public double totalElevation() {
            //Total elevation of all pixels in subbasin.

            double totalEl = 0;
            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    totalEl += lsuData.totalElevation;
                }
            }
            return totalEl;
        }

        // Total slope values of all pixels in subbasin.
        public double totalSlope() {
            var totalSl = 0.0;
            foreach (var channelData in this.getLsus().Values) {
                foreach (var lsuData in channelData.Values) {
                    totalSl += lsuData.totalSlope;
                }
            }
            return totalSl;
        }

        public void copyLsus() {
            //Make deep copy mergedLsus of lsus.  deepcopy in copy module does not do this adequately.

            this.mergedLsus = new Dictionary<int, Dictionary<int, LSUData>>();
            foreach (var (channel, channelData) in this.lsus) {
                this.mergedLsus[channel] = new Dictionary<int, LSUData>();
                foreach (var (landscape, lsuData) in channelData) {
                    this.mergedLsus[channel][landscape] = lsuData.copy();
                }
            }
        }

        public static double channelArea(Dictionary<int, LSUData> channelData) {
            //Return area (land and water) draining to channel: the sum of the areas of the channel's landscape units.

            double areaCh = 0;

            foreach (var lsuData in channelData.Values) {
                areaCh += lsuData.area;
            }
            return areaCh;
        }

        // Find the dominant key for a dictionary table of numeric values, 
        //         i.e. the key to the largest value, and the largest value
        //         
        public static (int, double) dominantKey(Dictionary<int, double> table) {
            var maxKey = -1;
            var maxVal = 0.0;
            foreach (var kvp in table) {
                var key = kvp.Key;
                var val = kvp.Value;
                if (val > maxVal) {
                    maxKey = key;
                    maxVal = val;
                }
            }
            return (maxKey, maxVal);
        }
    }



    public class ReachData {
        //Location and elevation of points at ends of reach, 
        // draining from upper to lower.

        public double upperX;
        public double upperY;
        public double upperZ;
        public double lowerX;
        public double lowerY;
        public double lowerZ;

        public ReachData(double x1, double y1, double z1, double x2, double y2, double z2) {
            //Initialise class variables
            // x coordinate of upper end
            this.upperX = x1;
            // y coordinate of upper end
            this.upperY = y1;
            // elevation of upper end
            this.upperZ = z1;
            // x coordinate of lower end
            this.lowerX = x2;
            // y coordinate of lower end
            this.lowerY = y2;
            // elevation of lower end
            this.lowerZ = z2;
        }
    }

    public class MergedChannelData {
        //Drainage area in square metres, length in metres, and slope in m/m for a collection of merged channels.
        //

        public double areaC;
        public int order;
        public double length;
        public double slope;
        public double minEl;
        public double maxEl;

        public MergedChannelData(double areaC, int order, double length, double slope, double minEl, double maxEl) {
            //Initialise class variables for first channel
            // total drainage area in square metres
            this.areaC = areaC;
            // Strahler order
            this.order = order;
            // total length in metres
            this.length = length;
            // mean slope in m/m for merged channels
            this.slope = slope;
            // minimum elevation in metres
            this.minEl = minEl;
            // maximum elevation in metres
            this.maxEl = maxEl;
        }

        public void add(double areaC, int order, double length, double slope, double minEl, double maxEl) {
            //Add a channel's data
            this.areaC = Math.Max(this.areaC, areaC);
            this.order = Math.Max(this.order, order);
            if (length > 0) {
                var newLength = this.length + length;
                this.slope = (this.slope * this.length + slope * length) / newLength;
                this.length = newLength;
            }
            this.minEl = Math.Min(this.minEl, minEl);
            this.maxEl = Math.Max(this.maxEl, maxEl);
        }
    }


    public class LakeData {
        //Data about lake defined by shapefile
        public Dictionary<int, (int, double[], double)> inChLinks;
        public HashSet<int> lakeChLinks;
        public int outChLink;
        public (int, int, double[], double) outPoint;
        public HashSet<int> otherOutChLinks;
        public double area;
        public double overrideArea;
        public double elevation;
        public double[] centroid;
        public WaterRole waterRole;

        public LakeData(double area, double overrideArea, double[] centroid, WaterRole waterRole) {
            // linknos of inflowing channels mapped to point id, point and elevation of channel outlet
            this.inChLinks = new Dictionary<int, (int, double[], double)>();
            // linknos of channels within lake
            this.lakeChLinks = new HashSet<int>();
            // linkno of outflowing stream. -1 means no outlet stream; outlet is a main outlet
            this.outChLink = -1;
            // subbasin, point id, point and elevation of outflow point (only used for reservoirs, ponds and wetlands)
            this.outPoint = (-1, -1, null, 0);
            // linknos of other outflowing channels
            this.otherOutChLinks = new HashSet<int>();
            // area in square metres
            this.area = area;
            // override area from Lakes shapefile area field (defaults to area)
            this.overrideArea = overrideArea;
            // elvation (mean elevation of incoming stream outlets, else outlet if no inlets, else elevation of centroid for playa)
            this.elevation = 0;
            //centroid
            this.centroid = centroid;
            // reservoir, pond, wetland or playa
            this.waterRole = waterRole;
        }
    }
}

