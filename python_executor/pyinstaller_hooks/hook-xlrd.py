from PyInstaller.utils.hooks import collect_submodules

# Collect all xlrd submodules
hiddenimports = collect_submodules('xlrd')

# Add essential xlrd modules that might not be captured
essential_modules = [
    'xlrd.biffh',
    'xlrd.formatting',
    'xlrd.info',
]
for mod in essential_modules:
    if mod not in hiddenimports:
        hiddenimports.append(mod)
