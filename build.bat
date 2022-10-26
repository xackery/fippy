@echo off
echo building....
"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\msbuild.exe" /property:Configuration=Release;Version=1.0.13.0
copy launcher\bin\Release\emulauncher.exe bin\fippy.exe

