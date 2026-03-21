from PyInstaller.utils.hooks import collect_submodules

# Collect all pandas submodules
hiddenimports = collect_submodules('pandas')

# Add essential pandas internal modules
essential_modules = [
    'pandas._libs',
    'pandas._libs.hashtable',
    'pandas._libs.index',
    'pandas._libs.internals',
    'pandas._libs.tslibs',
    'pandas._libs.window',
    'pandas.util',
    'pandas.compat',
    'pandas.compat.numpy',
]
for mod in essential_modules:
    if mod not in hiddenimports:
        hiddenimports.append(mod)
