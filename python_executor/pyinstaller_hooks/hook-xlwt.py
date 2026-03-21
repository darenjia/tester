from PyInstaller.utils.hooks import collect_submodules

# Collect all xlwt submodules
hiddenimports = collect_submodules('xlwt')

# Add essential xlwt modules
essential_modules = [
    'xlwt.Workbook',
    'xlwt.Worksheet',
    'xlwt.Row',
    'xlwt.Column',
]
for mod in essential_modules:
    if mod not in hiddenimports:
        hiddenimports.append(mod)
