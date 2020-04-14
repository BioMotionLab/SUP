using System.IO;
using System.Runtime.InteropServices;
using HDF.PInvoke;
using UnityEngine;

namespace MoshPlayer.Scripts.FileLoaders {
    public class OpenedH5DataSet {
        readonly long fileId;
        readonly long datasetId;
        readonly long typeId;
        readonly long spaceID;

        readonly int[] dimensions;
        public int[] Dimensions => dimensions;
    
        readonly ulong[] maxDimensions = {10000, 10000, 10000};

        public OpenedH5DataSet(string filePath, string datasetName) {
            if (!File.Exists(filePath)) Debug.Log( $"Didn't find file: {filePath}");

            fileId = H5F.open(filePath, H5F.ACC_RDONLY);
            datasetId = H5D.open(fileId, datasetName);
            typeId = H5D.get_type(datasetId);

            spaceID = H5D.get_space(datasetId);
            int ndims = H5S.get_simple_extent_ndims(spaceID);
            if (ndims >= 0) {
                ulong[] dims = new ulong[ndims];
                H5S.get_simple_extent_dims(spaceID, dims, maxDimensions);
                dimensions = ConvertDimensions(dims);
            }
        }

        int[] ConvertDimensions(ulong[] dims) {
            int[] dimensions = new int[dims.Length];
            for (int i = 0; i < dims.Length; i++) {
                dimensions[i] = (int) dims[i];
            }
            return dimensions;
        }

        public T[] LoadAs<T>(int length) {
            T[] array = new T[length];
            GCHandle gch = GCHandle.Alloc(array, GCHandleType.Pinned);
            try {
                H5D.read(datasetId, typeId, H5S.ALL, H5S.ALL, H5P.DEFAULT, gch.AddrOfPinnedObject());
            }
            finally {
                gch.Free();
                H5S.close(spaceID);
                H5D.close(datasetId);
                H5F.close(fileId);
            }
        
            return array;
        }
    
    }
}