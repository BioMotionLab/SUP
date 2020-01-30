#%%
import numpy as np
import json
import sys
# noinspection PyPep8Naming
from scipy.spatial.transform import Rotation

#%%
npzFileName = 'AMASS_Data/20160330_03333/punching_poses.npz'

#%%
print(sys.version)
#%%


class AMASSJsonDataConverter:
    JOINTS = 52
    ROTATION_VECTOR_DIMENSIONS = 3
    QUATERNION_DIMENSIONS = 4

    gender: np.ndarray
    betas: np.ndarray
    poses: np.ndarray
    dmpls: np.ndarray
    trans: np.ndarray
    frames: int
    poses_as_quaternion: np.ndarray

    def __init__(self, npz_file_path: str):
        self.npzFile = npz_file_path
        self.data = self.load_data()
        self.read_data()

        print(f'poses shape: {self.poses.shape}')
        print(f'poses[0] shape {self.poses[0].shape}')
        reshaped_poses = np.reshape(self.poses, [self.poses.shape[0], self.JOINTS, self.ROTATION_VECTOR_DIMENSIONS])
        self.convert_poses_to_quaternions(reshaped_poses)
        print(f'poses as quat shape {self.poses_as_quaternion.shape}')
        print(f'p as q [0]\n {self.poses_as_quaternion[0]}')

        self.data_as_dict = {
            "gender": self.gender,
            "trans": self.trans,
            "poses": self.poses_as_quaternion,
            "betas": self.betas,
            "dmpls": self.dmpls,
        }

    def convert_poses_to_quaternions(self, reshaped_poses):
        poses_as_quaternion = np.empty([self.frames, self.JOINTS, self.QUATERNION_DIMENSIONS])
        for frameIndex in range(0, self.frames):
            for poseIndex in range(0, self.JOINTS):
                rotation_vector_raw = reshaped_poses[frameIndex][poseIndex]
                rotation = Rotation.from_rotvec(rotation_vector_raw)
                quaternion = rotation.as_quat()
                poses_as_quaternion[frameIndex][poseIndex] = quaternion
        self.poses_as_quaternion = poses_as_quaternion

    @staticmethod
    def default_encoding(obj):
        if type(obj).__module__ == np.__name__:
            if isinstance(obj, np.ndarray):
                return obj.tolist()
            else:
                return obj.item()
        raise TypeError('Unknown type:', type(obj))

    def write_to_json(self, json_path: str):
        dumped = json.dumps(self.data_as_dict, default=self.default_encoding, indent=4)
        with open(json_path, 'w') as f:
            f.write(dumped)

    def read_data(self):
        self.gender = self.data['gender'].astype(str)
        self.betas = self.data['betas']
        self.poses = self.data['poses']
        self.dmpls = self.data['dmpls']
        self.trans = self.data['trans']
        self.frames = self.poses.shape[0]
        print(f'gender: {self.gender}')
        print(f'betas: {self.betas.shape}')
        print(f'poses: {self.poses.shape}')
        print(f'dmpls: {self.dmpls.shape}')
        print(f'trans: {self.trans.shape}')
        print(f'frames detected: {self.frames}')

    def load_data(self):
        # noinspection PyBroadException
        try:
            data = np.load(self.npzFile)
            return data
        except Exception:
            print(f'Could not read {npzFileName}! Skipping...')


#%%
converter = AMASSJsonDataConverter(npzFileName)
#%%
converter.write_to_json('test_data.json')
print("done conversion.")

#%%
sample = "amass_sample.npz"
converter = AMASSJsonDataConverter(sample)
converter.write_to_json('amass_sample.json')



#%%
sample = "amass_sample.npz"
converter = AMASSJsonDataConverter(sample)
print(converter.poses[0])
