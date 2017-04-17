set -x

FolderName=$(pwd)

echo \#\#\# Build
cd $FolderName
cd Build
dotnet restore
dotnet build
# Set ConnectionString
set +x
dotnet run 01 "$ConnectionString" 
set -x
# InstallAll
dotnet run 02 

# Build RunSql
echo \#\#\# RunSql
cd $FolderName
cd Build
dotnet run 11

# Publish Server
echo \#\#\# Publish Server
cd $FolderName
cd Server
dotnet restore
dotnet build
dotnet publish

# Deploy
cd $FolderName
echo \#\#\# Deploy
./Submodule/Framework/Build/Shell/travis-ci.org/Deploy.sh
