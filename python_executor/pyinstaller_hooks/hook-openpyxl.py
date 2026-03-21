from PyInstaller.utils.hooks import collect_submodules

# Collect all openpyxl submodules
hiddenimports = collect_submodules('openpyxl')

# Add essential openpyxl modules
essential_modules = [
    'openpyxl.cell',
    'openpyxl.workbook',
    'openpyxl.worksheet',
]
for mod in essential_modules:
    if mod not in hiddenimports:
        hiddenimports.append(mod)
