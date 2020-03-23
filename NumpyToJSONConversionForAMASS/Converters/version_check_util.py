def check_version(package, v,  needed):
    vers = tuple(map(int, (v.split("."))))
    needed_vers = tuple(map(int, (needed.split("."))))
    if vers < needed_vers:
        print(f"{package} version {v}, Tested with {needed}, you may need to update.")
    else:
        print(f"{package} version {v}, good")