

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using OSGeo.GDAL;

namespace ArcSWATPlus { 
    
    // 
    //     Read and write rasters split into pieces (chunks) as necessary to fit into memory.
    //     
    //     Chunks are blocks of contiguous rows.
    //     In the interest of efficiency, this is limited to only using band 1 (to avoid arrays of buffers etc).
    //     
    public class Raster {
        
        public GlobalVars _gv;

        public int[] i_array;
        
        public float[] f_array;
        
        public bool arrayChanged;
        
        public Band band;
        
        public bool canWrite;
        
        public int chunkCount;
        
        public Dictionary<int, Chunk> chunks;
        
        public int currentIndex;
        
        public Dataset ds;
        
        public string fileName;
        
        public bool isInt;
        
        public double noData;
        
        public int numCols;
        
        public int numRows;
        
        public Raster(string fileName, GlobalVars gv, bool canWrite = false, bool isInt = true) {
            //# path to raster file (.tif)
            this.fileName = fileName;
            this._gv = gv;
            //# number of chunks to be used when reading and writing raster
            this.chunkCount = 0;
            //# chunks
            this.chunks = new Dictionary<int, Chunk>();
            //# current chunk index: in domain of chunks indicates has data
            this.currentIndex = -1;
            //# current array of values
            this.i_array = null;
            this.f_array = null;
            //# flag set if array data potentially changed
            this.arrayChanged = false;
            //# rows in whole raster
            this.numRows = 0;
            //# columns in whole raster
            this.numCols = 0;
            //# can be written
            this.canWrite = canWrite;
            //# true for integer raster, false for float
            this.isInt = isInt;
            //# dataset
            this.ds = null;
            //# band 1
            this.band = null;
            //# no data value
            this.noData = -1;
            //self.readCount = 0
            //self.writeCount = 0
        }
        
        // 
        //         Open raster for reading or writing.
        //         
        //         Return true if successful, raise exception if memory error, false if failure.
        //         
        public virtual bool open(
            int chunkCount,
            int numRows = 0,
            int numCols = 0,
            double[] transform = null,
            OSGeo.OSR.SpatialReference projection = null,
            double noData = -1) {
            int overlap;
            this._gv.openRasters.Add(this);
            try {
                if (this.canWrite) {
                    if (File.Exists(this.fileName)) {
                        this.ds = Gdal.Open(this.fileName, Access.GA_Update);
                    } else {
                        var typ = this.isInt ? DataType.GDT_Int32 : DataType.GDT_Float32;
                        this.ds = Gdal.GetDriverByName("GTiff").Create(this.fileName, numCols, numRows, 1, typ, null);
                    }
                    Debug.Assert(this.ds is not null);
                    this.numRows = numRows;
                    this.numCols = numCols;
                    this.ds.SetGeoTransform(transform);
                    this.ds.SetSpatialRef(projection);
                    this.band = this.ds.GetRasterBand(1);
                    Debug.Assert(this.band is not null);
                    this.arrayChanged = false;
                    this.band.SetNoDataValue(noData);
                    this.noData = noData;
                } else {
                    this.ds = Gdal.Open(this.fileName, Access.GA_ReadOnly);
                    Debug.Assert(this.ds is not null);
                    this.numRows = this.ds.RasterYSize;
                    this.numCols = this.ds.RasterXSize;
                    this.band = this.ds.GetRasterBand(1);
                    Debug.Assert(this.band is not null);
                    this.arrayChanged = false;
                    int hasVal;
                    this.band.GetNoDataValue(out this.noData, out hasVal);
                }
                int xBlockSize, yBlockSize;
                this.band.GetBlockSize(out xBlockSize, out yBlockSize);
                this.chunkCount = chunkCount;
                int chunkSize;
                if (chunkCount <= 1) {
                    // avoid accidentally dividing by zero later
                    chunkSize = this.numRows;
                    overlap = 0;
                } else {
                    //                 chunkSize = self.numRows // self.chunkCount
                    //                 # increase chunkSize to be multiple of yBlockSize, since this improves efficiency.
                    //                 # Allow increase of at most 20%,
                    //                 # trying to avoid danger of this array size being same as one with smaller chunkCount,
                    //                 # so using no less memory.
                    //                 if chunkSize % yBlockSize != 0:
                    //                     newSize = ((chunkSize // yBlockSize) + 1) * yBlockSize
                    //                     if float(newSize) / chunkSize <= 1.2:
                    //                         chunkSize = newSize
                    //                     else:
                    //                         Utils.loginfo(u'Failed to increase chunkSize {0} to be multiple of yBlockSize {1}'.format(chunkSize, yBlockSize))
                    //                 overlap = 0
                    // For count > 1, chunk size approximates to numrows / count + 10% / (count -1)
                    // so that chunks overlap.  An overlap should reduce switching between chunks when a flow path runs more or less
                    // horizontally along a chunk boundary.
                    chunkSize = this.numRows / this.chunkCount + this.numRows / ((this.chunkCount - 1) * 10);
                    // increase chunkSize to be multiple of yBlockSize, since this improves efficiency.
                    // Allow increase of at most 20%,
                    // trying to avoid danger of this array size being same as one with smaller chunkCount,
                    // so using no less memory.
                    if (chunkSize % yBlockSize != 0) {
                        int newSize = (chunkSize / yBlockSize + 1) * yBlockSize;
                        if ((newSize) / chunkSize <= 1.2) {
                            chunkSize = newSize;
                            // overlap also a multiple of yBlockSize
                            overlap = (chunkSize * chunkCount - this.numRows) / ((chunkCount - 1) * yBlockSize) * yBlockSize;
                        } else {
                            Utils.loginfo(string.Format("Failed to increase chunkSize {0} to be multiple of yBlockSize {1}", chunkSize, yBlockSize));
                            // chunkSize not a multiple of yBlockSize, so no point in trying to make overlap a multiple
                            overlap = (chunkSize * chunkCount - this.numRows) / (chunkCount - 1);
                        }
                    } else {
                        // overlap also a multiple of yBlockSize
                        overlap = (chunkSize * chunkCount - this.numRows) / ((chunkCount - 1) * yBlockSize) * yBlockSize;
                    }
                }
                // Raster value at (row, col) is array[col + row * numCols]
                for (int i = 0; i < chunkCount; i++) {
                    var rowOffset = (chunkSize - overlap) * i;
                    var rowCount = i == chunkCount - 1 ? this.numRows - rowOffset : chunkSize;
                    var chunk = new Chunk(rowCount, rowOffset);
                    this.chunks[i] = chunk;
                }
                // we allocate space for the array early since we want to generate any memory exception early
                // fill all chunks with noData
                if (this.isInt) {
                    this.i_array = new int[chunkSize * this.numCols];
                    System.Array.Fill<int>(this.i_array, (int)noData);
                } else {
                    this.f_array = new float[chunkSize * this.numCols];
                    System.Array.Fill<float>(this.f_array, (float)noData);
                }
                if (this.canWrite) {
                    if (this.isInt) {
                        foreach (var ch in this.chunks.Values) {
                            this.band.WriteRaster(0, ch.rowOffset, this.numCols, this.numRows, this.i_array, this.numCols, ch.numRows, 0, 0);
                        }
                    } else {
                        
                        foreach (var ch in this.chunks.Values) {
                            this.band.WriteRaster(0, ch.rowOffset, this.numCols, this.numRows, this.f_array, this.numCols, ch.numRows, 0, 0);
                        }
                    }
                    this.currentIndex = 0;
                } else {
                    // load a chunk and set the current index
                    this.read(0, 0);
                }
                return true;
            } catch (OutOfMemoryException) {
                throw;
            } catch (Exception) {
                Utils.exceptionError(string.Format("Failed to open raster {0}", this.fileName), this._gv.isBatch);
                return false;
            }
        }

        // Return raster value at [row, col], which is array[chunkrow * numCols + col],
        // where chunkRow is row - chunk.rowOffset
        public virtual double read(int row, int col) {
            if (0 > col || col >= this.numCols) {
                return this.noData;
            }
            Chunk chunk;
            int index;
            if (!this.chunks.TryGetValue(this.currentIndex, out chunk)) {
                index = -1;
            } else {
                index = chunk is not null && (chunk.rowOffset <= row && row < chunk.rowOffset + chunk.numRows) ? this.currentIndex : -1;
            }
            if (index == -1) {
                foreach (var (i, ch) in this.chunks) {
                    if (ch.rowOffset <= row && row < ch.rowOffset + ch.numRows) {
                        index = i;
                        chunk = ch;
                        break;
                    }
                }
                if (index == -1) {
                    //Utils.error(u'Failed to read row {0} column {1} of raster {2}'.format(row, col, self.fileName), self._gv.isBatch)
                    return this.noData;
                }
            }
            Debug.Assert(chunk is not null);
            if (this.currentIndex != index) {
                Debug.Assert(this.band is not null);
                if (this.canWrite && this.arrayChanged) {
                    var currentChunk = this.chunks[this.currentIndex];
                    if (this.isInt) {
                        this.band.WriteRaster(0, currentChunk.rowOffset, this.numCols, this.numRows, this.i_array, this.numCols, currentChunk.numRows, 0, 0);
                    } else {
                        this.band.WriteRaster(0, currentChunk.rowOffset, this.numCols, this.numRows, this.f_array, this.numCols, currentChunk.numRows, 0, 0);
                    }
                    //self.writeCount += 1
                    this.arrayChanged = false;
                }
                if (this.isInt) {
                    this.band.ReadRaster(0, chunk.rowOffset, this.numCols, this.numRows, this.i_array, this.numCols, chunk.numRows, 0, 0);
                } else {
                    this.band.ReadRaster(0, chunk.rowOffset, this.numCols, this.numRows, this.f_array, this.numCols, chunk.numRows, 0, 0);
                }
                //self.readCount += 1
                this.currentIndex = index;
            }
            //Debug.Assert(this.array is not null);
            if (this.isInt) {
                return this.i_array[(row - chunk.rowOffset) * this.numCols + col];
            } else {
                return this.f_array[(row - chunk.rowOffset) * this.numCols + col];
            }
        }
        
        // Write val at [row, col].
        public virtual void write(int row, int col, object val) {
            if (0 > col || col >= this.numCols) {
                return;
            }
            if (!this.canWrite) {
                Utils.error(string.Format("Trying to write to readonly raster {0}", this.fileName), this._gv.isBatch);
                return;
            }
            Chunk chunk;
            int index;
            if (!this.chunks.TryGetValue(this.currentIndex, out chunk)) {
                index = -1;
            } else {
                index = chunk is not null && (chunk.rowOffset <= row && row < chunk.rowOffset + chunk.numRows) ? this.currentIndex : -1;
            }
            if (index == -1) {
                foreach (var (i, ch) in this.chunks) {
                    if (ch.rowOffset <= row && row < ch.rowOffset + ch.numRows) {
                        index = i;
                        chunk = ch;
                        break;
                    }
                }
                if (index == -1) {
                    //Utils.error(u'Failed to read row {0} column {1} of raster {2}'.format(row, col, self.fileName), self._gv.isBatch)
                    return;
                }
            }
            Debug.Assert(chunk is not null);
            if (this.currentIndex != index) {
                Debug.Assert(this.band is not null);
                if (this.arrayChanged) {
                    var currentChunk = this.chunks[this.currentIndex];
                    if (this.isInt) {
                        this.band.WriteRaster(0, currentChunk.rowOffset, this.numCols, this.numRows, this.i_array, this.numCols, currentChunk.numRows, 0, 0);
                    } else {
                        this.band.WriteRaster(0, currentChunk.rowOffset, this.numCols, this.numRows, this.f_array, this.numCols, currentChunk.numRows, 0, 0);
                    }
                    //self.writeCount += 1
                    this.arrayChanged = false;
                }
                if (this.isInt) {
                    this.band.ReadRaster(0, chunk.rowOffset, this.numCols, this.numRows, this.i_array, this.numCols, chunk.numRows, 0, 0);
                } else {
                    this.band.ReadRaster(0, chunk.rowOffset, this.numCols, this.numRows, this.f_array, this.numCols, chunk.numRows, 0, 0);
                }
                //self.readCount += 1
                this.currentIndex = index;
            }
            if (this.isInt) {
                this.i_array[(row - chunk.rowOffset) * this.numCols + col] = Convert.ToInt32(val);
            } else {
                this.f_array[(row - chunk.rowOffset) * this.numCols + col] = Convert.ToSingle(val);
            }
            this.arrayChanged = true;
        }
        
        // Write if necessary and release memory (which also flushes).
        public virtual void close() {
            if (this.canWrite) {
                if (this.arrayChanged) {
                    Debug.Assert(this.band is not null);
                    Chunk currentChunk = this.chunks[this.currentIndex];
                    if (this.isInt) {
                        this.band.WriteRaster(0, currentChunk.rowOffset, this.numCols, this.numRows, this.i_array, this.numCols, currentChunk.numRows, 0, 0);
                    } else {
                        this.band.WriteRaster(0, currentChunk.rowOffset, this.numCols, this.numRows, this.f_array, this.numCols, currentChunk.numRows, 0, 0);
                    }
                    this.arrayChanged = false;
                }
            }
            //    self.writeCount += 1
            //if self.canWrite:
            //    Utils.loginfo(u'{0} chunks of {3} rows read {1} times and written {2} times'.format(self.fileName, self.readCount, self.writeCount, self.chunks[0].numRows))
            //else:
            //    Utils.loginfo(u'{0} chunks of {2} rows read {1} times'.format(self.fileName, self.readCount, self.chunks[0].numRows))
            this.release();
        }
        
        // Release memory (which also flushes).
        public virtual void release() {
            // resetting currentIndex and arrayChanged allows a Raster object to be closed and reopened:
            // opening will start with a read
            if (this.ds is not null) { this.ds.Dispose(); }
            this.chunks = new Dictionary<int, Chunk>();
            this.currentIndex = -1;
            this.arrayChanged = false;
            this.i_array = null;
            this.f_array = null;
            this.band = null;
            this.ds = null;
        }
    }
    
    // Piece of a raster: a number of rows starting from rowOffset.
    public class Chunk {
        
        public int numRows;
        
        public int rowOffset;
        
        public Chunk(int numRows, int rowOffset) {
            //# rows in chunk
            this.numRows = numRows;
            //# row offset
            this.rowOffset = rowOffset;
        }
    }

}
