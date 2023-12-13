import os
path = "."
files = os.listdir(path)

for file in files:
    if file.endswith(".fleet"):
        if file[:-6] + '.txt' in files:
            os.remove(file[:-6] + '.txt')
        os.rename(file, file[:-6] + '.txt')
