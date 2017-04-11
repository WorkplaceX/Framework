PATH="/home/travis/.dotnet":$PATH
cd Build
dotnet restore
dotnet build
dotnet run 01 "$ConnectionString"
dotnet run 02
