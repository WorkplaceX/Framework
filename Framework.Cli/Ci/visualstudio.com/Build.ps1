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
	if ($lastexitcode -ne 0) { exit $lastexitcode }

	# Build
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- build
	if ($lastexitcode -ne 0) { exit $lastexitcode }

	# Deploy
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- deploy
	if ($lastexitcode -ne 0) { exit $lastexitcode }
}

# Init
$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"

Main