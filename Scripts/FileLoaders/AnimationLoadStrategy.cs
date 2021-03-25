using System;
using System.Threading.Tasks;
using SMPLModel;

namespace FileLoaders {
    public abstract class AnimationLoadStrategy {
       
        AnimationData animationData;
        readonly Models possibleModels;
        
        protected AnimationLoadStrategy( Models possibleModels) {
            
            if (possibleModels == null) throw new NullReferenceException("No Models specified");
            this.possibleModels = possibleModels;
        }

        public async Task<AnimationData> LoadData() {
            animationData = await LoadDataWithStrategy();
            FindCorrectModel();
            FormatData();
            return animationData;
        }

        void FindCorrectModel() {
            foreach (ModelDefinition model in possibleModels.ModelDefinitions) {
                if (!IsMatchingModel(model)) continue;
                animationData.Model = model;
                //Debug.Log($"Matched model {matchedModel.ModelName}");
                break;
            }
            
            if (animationData.Model == null) 
                throw new NullReferenceException("Could not match animation to a model");
            
            if (animationData.Model.ModelName == "SMPL") 
                throw new DataReadException("Loaded data appears to be from older SMPL model. supporting SMPL was taking up a lot of development time, " +
                                            "so I disabled it. If you need this functionality, please contact me and I can get it working -Adam");
        }

        protected abstract Task<AnimationData> LoadDataWithStrategy();

        protected abstract bool IsMatchingModel(ModelDefinition model);

        protected abstract void FormatData();
        

        public class DataReadException : Exception {

            public DataReadException() {
            }

            public DataReadException(string e) : base(e) {
            }

        }
    }
}