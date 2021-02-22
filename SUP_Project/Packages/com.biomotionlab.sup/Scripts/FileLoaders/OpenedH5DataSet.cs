using System.Runtime.InteropServices;
using HDF.PInvoke;

namespace FileLoaders {
    public class OpenedH5DataSet {
        readonly long fileId;
        readonly string datasetName;

        readonly ulong[] maxDimensions = {10000, 10000, 10000};
        int[] dimensions;

        public int[] Dimensions => dimensions;

        public OpenedH5DataSet(long fileId, string datasetName) {
            this.fileId = fileId;
            this.datasetName = datasetName;
            
            long datasetId = H5D.open(fileId, datasetName);
            try {
                H5D.get_type(datasetId);

                long spaceID = H5D.get_space(datasetId);
                int ndims = H5S.get_simple_extent_ndims(spaceID);
                if (ndims >= 0) {
                    ulong[] dims = new ulong[ndims];
                    H5S.get_simple_extent_dims(spaceID, dims, maxDimensions);
                    dimensions = ConvertDimensions(dims);
                }
            }
            finally {
                H5D.close(datasetId);
            }
           
            
        }

        int[] ConvertDimensions(ulong[] dims) {
            int[] convertedDimensions = new int[dims.Length];
            for (int i = 0; i < dims.Length; i++) {
                convertedDimensions[i] = (int) dims[i];
            }
            return convertedDimensions;
        }

        public T[] LoadAs<T>(int length) {
            T[] array = new T[length];
            GCHandle gch = GCHandle.Alloc(array, GCHandleType.Pinned);
            
            long dataSetID = H5D.open(fileId, datasetName);
            long typeId = H5D.get_type(dataSetID);
            try {
                H5D.read(dataSetID, typeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, gch.AddrOfPinnedObject());
            }
            finally {
                gch.Free();
                H5D.close(dataSetID);
            }
        
            return array;
        }
    
    }
}