from PyInstaller.utils.hooks import collect_submodules

# Collect all numpy submodules
hiddenimports = collect_submodules('numpy')

# Add essential numpy internal modules
essential_modules = [
    'numpy._utils',
    'numpy._distributor_init',
    'numpy.compat',
    'numpy.core',
    'numpy.core.multiarray',
    'numpy.core.umath',
    'numpy.linalg',
    'numpy.random',
]
for mod in essential_modules:
    if mod not in hiddenimports:
        hiddenimports.append(mod)
