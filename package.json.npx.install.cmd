:; echo "Running on Linux"; echo "Installed .NET Core Version:"; dotnet --version; echo "Build..."; dotnet run --project "$0/../../lib/node_modules/workplacex-cli/Framework.Cli" -- $@; exit $?

echo "Running on Windows"
echo "Installed .NET Core Version:"
dotnet --version
echo "Build..."
dotnet run --project "%dp0%node_modules/workplacex-cli/Framework.Cli" -- %*