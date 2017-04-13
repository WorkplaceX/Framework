set -x

FolderName=$(pwd)

echo \#\#\# Build
cd $FolderName
cd Build
dotnet restore
dotnet build
dotnet run 06
