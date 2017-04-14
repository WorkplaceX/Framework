set -x

FolderName=$(pwd)

echo \#\#\# Build
cd $FolderName
cd Build
dotnet restore
dotnet build
dotnet run 06 2> Error.txt
echo "Error2 Start"
echo "$(<Error.txt)"
echo "Error2 End"

