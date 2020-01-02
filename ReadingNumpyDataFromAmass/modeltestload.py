#%%
import numpy as np
import json
# noinspection PyPep8Naming
from scipy.spatial.transform import Rotation

#%%
npzFileName = 'data/model_m.npz'

#%%


def load_data(path):
    # noinspection PyBroadException
    try:
        loaded_data = np.load(path)
        return loaded_data
    except Exception:
        print(f'Could not read {path}! Skipping...')


#%%
data = load_data(npzFileName)
print(data.files)

#%%
print(f"J_regressor_prior: {data['J_regressor_prior'].shape}")
print(f"f: {data['f'].shape}")
print(f"J_regressor: {data['J_regressor'].shape}")
print(f"kintree_table: {data['kintree_table'].shape}")
print(f"J: {data['J'].shape}")
print(f"weights_prior: {data['weights_prior'].shape}")
print(f"weights: {data['weights'].shape}")
print(f"posedirs: {data['posedirs'].shape}")
print(f"v_template: {data['v_template'].shape}")
print(f"shapedirs: {data['shapedirs'].shape}")
