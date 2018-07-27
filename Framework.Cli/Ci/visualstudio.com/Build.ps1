param($AzureGitUrl, $ConnectionString)

# dotnet, npm, node version check
dotnet --version
npm --version
node --version

function Main
{
	# Cli build
	cd $FolderName
	cd Application.Cli
	dotnet build

	# Config
    dotnet run --no-build -- config azureGitUrl="$AzureGitUrl" # Set AzureGitUrl
	if %errorlevel% NEQ 0 exit %errorlevel%

	# Build
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- build
	if %errorlevel% NEQ 0 exit %errorlevel%

	# Deploy
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- deploy
	if %errorlevel% NEQ 0 exit %errorlevel%
}

# Init
$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"

Main