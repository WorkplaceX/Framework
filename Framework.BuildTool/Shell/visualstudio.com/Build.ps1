param($ConnectionString, $AzureGitUrl)

# Init
$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"

# Version
echo "### Dotnet Version"
dotnet --version

# ConnectionString
echo "### BuildTool ConnectionString"
cd $FolderName
cd BuildTool
dotnet run -- connection "$ConnectionString" # Set ConnectionString

# BuildTool InstallAll
echo "BuildTool InstallAll"
cd $FolderName
cd BuildTool
dotnet run --no-build -- installAll

# BuildTool UnitTest
echo "### BuildTool UnitTest"
cd $FolderName
cd BuildTool
dotnet run --no-build -- unitTest

# BuildTool RunSqlCreate 
echo "### BuildTool RunSqlCreate"
cd $FolderName
cd BuildTool
# dotnet run --no-build -- runSqlCreate # Run sql update manually from BuildTool CLI because of database firewall.

# BuildTool Deploy
echo "### BuildTool Deploy"
cd $FolderName
cd BuildTool
dotnet run --no-build -- deploy "$AzureGitUrl" # publish
