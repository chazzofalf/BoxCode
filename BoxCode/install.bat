dotnet publish -r win-x64
if [%1]==[] goto NOPE
mkdir "%1"\dist

xcopy bin\Release\net8.0\win-x64\publish\* "%1"\dist\
:NOPE
echo "Please specify and install directory."
:DONE