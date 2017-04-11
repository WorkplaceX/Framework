cd Build
/home/travis/.dotnet/dotnet restore
/home/travis/.dotnet/dotnet build
/home/travis/.dotnet/dotnet run 01 "$ConnectionString"
/home/travis/.dotnet/dotnet run 02
