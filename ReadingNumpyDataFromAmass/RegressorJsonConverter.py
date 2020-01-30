#%%
import numpy as np
import json
import sys
# noinspection PyPep8Naming

#%%
model_path = 'data/model_smplh_mana_m.npz'

#%%
print(sys.version)
#%%


class AMASSJsonRegressorConverter:
    JOINTS = 52

    J_regressor: np.ndarray
    v_template: np.ndarray
    shapedirs: np.ndarray
    joint_regressor: np.ndarray
    joint_template: np.ndarray
    data_as_dict: dict
    model_path: str

    def __init__(self, model_path: str):
        self.model_path = model_path
        self.data = self.load_data()
        self.read_data()

    def load_data(self):
        # noinspection PyBroadException
        try:
            data = np.load(self.model_path)
            return data
        except Exception:
            print(f'Could not read {model_path}! Skipping...')

    def read_data(self):
        self.v_template = self.data['v_template']
        self.J_regressor = self.data['J_regressor']
        self.shapedirs = self.data['shapedirs']

        self.joint_template = self.J_regressor.dot(self.v_template)
        self.joint_regressor = np.einsum('ij,jkl->ikl', self.J_regressor, self.shapedirs)

        print(f'joint_template: {self.joint_template.shape}')
        print(f'joint_regressor: {self.joint_regressor.shape}')

        self.data_as_dict = {
            "joint_template": self.joint_template,
            "joint_regressor": self.joint_regressor,
        }

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


#%%
converter = AMASSJsonRegressorConverter(model_path)
#%%
converter.write_to_json('test_regressor.json')
print("done conversion.")

#%%
betas = np.array([ 2.2140,  2.0062,  1.7169, -1.6117,  0.5180,  1.4124, -0.1580, -0.1450,
          0.0671,  1.9010,  0.2068,  0.5701, -0.0117, -0.1653,  0.6465,  0.2017])
print(betas)

#%%
joint_locations_direct = converter.joint_template + converter.joint_regressor.dot(betas)
print(joint_locations_direct)

#%%
recentered_joint_locations = joint_locations_direct-joint_locations_direct[0]
print(recentered_joint_locations)
#%%
print(converter.joint_regressor.shape)
reshaped_regressor = converter.joint_regressor.reshape((156, 16))
print(reshaped_regressor.shape)
print(betas.shape)
#%%
multiplicaiton = reshaped_regressor.dot(betas)
print(multiplicaiton.shape)
#%%
reshaped_mult = multiplicaiton.reshape((52,3))
print(reshaped_mult.shape)
#%%
result = converter.joint_template + reshaped_mult
print(result)
#%%

np.set_printoptions(precision=5)
np.set_printoptions(suppress=True)

print(np.around(joint_locations_direct, 5))
print(np.around(joint_locations, 5))

#%%
print(converter.joint_regressor.shape)

#%%
model = converter.data
v_template = model["v_template"]
#%%
print(v_template)
#%%

shapedirs = model["shapedirs"]

#%%
print(shapedirs.shape)
print(shapedirs)
#%%
J_regressor = model["J_regressor"]
print(J_regressor)
#%%

print(betas.shape)
#%%
bs = np.einsum('ijk,k->ij', shapedirs, betas)

#%%
#%%
V_deformed = v_template + bs
#%%
print(V_deformed.shape)
print(V_deformed)
#%%
print(J_regressor.shape)
#%%
joint_loc = J_regressor.dot(V_deformed)
#%%
print(joint_loc)

#%%
recentered_joint_loc = joint_loc - joint_loc[0]
print(recentered_joint_loc)

