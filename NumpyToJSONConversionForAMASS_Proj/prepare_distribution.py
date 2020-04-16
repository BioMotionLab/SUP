import os
import zipfile


def zip_directory(directory_path, zipfile_handle, excludes):

    lenDirPath = len(directory_path)
    # ziph is zipfile handle
    for root, dirs, allfiles in os.walk(directory_path):
        for exclude in excludes:
            if exclude in dirs:
                print(f'excluding folder: {exclude}')
                dirs.remove(exclude)
        for file in allfiles:
            print(f'processing file: {file}')

            if os.path.splitext(file)[0] in excludes or file in excludes:
                print(f'\texcluding file: {file}')

            else:
                print(f"\twriting: {file}")
                filePath = os.path.join(root, file)
                zipfile_handle.write(filePath, filePath[lenDirPath:])


#%%
if __name__ == '__main__':

    #%%
    zip_name = 'Build/npz_to_json_tools.zip'
    new_zip = zipfile.ZipFile(zip_name, 'w')

    #%%
    basedir = os.getcwd()
    print(os.getcwd())
    #%%
    excludes = ['venv', 'Build', 'Models', 'Regressor', 'ModelConverterExamples',
                '.gitignore', '.idea', 'prepare_distribution', zip_name]
    #%%

    zip_directory(basedir, new_zip, excludes)
    new_zip.close()
    print('\n*** DONE ***')
