import numpy as np
import json


# This class converts a model.npz into a Unity-readable Json file.
# See the AMASSConverterExamples file to see how it is used.
class SMPLHRegressorToJSON:

    # Initialize static-typed variables
    J_regressor: np.ndarray
    v_template: np.ndarray
    shapedirs: np.ndarray
    joint_regressor: np.ndarray
    joint_template: np.ndarray
    data_as_dict: dict
    model_path: str

    # Constructor
    def __init__(self, model_path: str):
        self.model_path = model_path
        self.data = self.load_data()
        self.read_data()

    # Load Data from .npz file
    def load_data(self):
        # noinspection PyBroadException
        try:
            data = np.load(self.model_path)
            return data
        except Exception:
            print(f'Could not read {self.model_path}! Skipping...')

    # Process data after load
    def read_data(self):
        self.v_template = self.data['v_template']
        self.J_regressor = self.data['J_regressor']
        self.shapedirs = self.data['shapedirs']

        # Python model stores things relative to vertices. Unity can't really access verts.
        # This will convert joint regressors to be independent of vertices, and only dependent on joints.
        # Doing so requires calculating then adding a joint template.
        self.joint_template = self.J_regressor.dot(self.v_template)
        self.joint_regressor = np.einsum('ij,jkl->ikl', self.J_regressor, self.shapedirs)

        print(f'joint_template: {self.joint_template.shape}')
        print(f'joint_regressor: {self.joint_regressor.shape}')

        # JSON is based on dictionaries, so need to convert to that.
        self.data_as_dict = {
            "joint_template": self.joint_template,
            "joint_regressor": self.joint_regressor,
        }

    # Set up JSON encoding format
    @staticmethod
    def default_encoding(obj):
        if type(obj).__module__ == np.__name__:
            if isinstance(obj, np.ndarray):
                return obj.tolist()
            else:
                return obj.item()
        raise TypeError('Unknown type:', type(obj))

    # Write dicts to json.
    def write_to_json(self, json_path: str):
        dumped = json.dumps(self.data_as_dict, default=self.default_encoding, indent=4)
        with open(json_path, 'w') as f:
            f.write(dumped)
