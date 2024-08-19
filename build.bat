@echo off
@rem If 'msbuild' is not recognized, please set compilation environment firstly

echo ===^> Build solution with GstarCAD 2024 or greater ...
msbuild -p:CADVersion=GstarCAD2024To2025 -p:Configuration=Release -p:Platform="Any CPU" -t:Restore -t:Rebuild -v:minimal

echo ===^> Build solution with GstarCAD 2023 or less ...
msbuild -p:CADVersion=GstarCAD2017To2023 -p:Configuration=Release -p:Platform="Any CPU" -t:Restore -t:Rebuild -v:minimal
