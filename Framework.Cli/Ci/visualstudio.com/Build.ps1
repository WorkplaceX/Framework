param($ConfigCliJson)

echo "### Build.ps1"

# powershell, dotnet, npm, node version check
echo $PSVersionTable.PSVersion
dotnet --version
npm --version
node --version

# Init
$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"
$FileNameErrorText = "$($FolderName)Error.txt";

function Main
{
	# Cli build
	echo "### Build.ps1 (Cli Build)"
	cd $FolderName
	cd Application.Cli
	dotnet build
	ErrorCheck

	# Config
	echo "### Build.ps1 (Config)"
	cd $FolderName
	cd Application.Cli
    dotnet run --no-build -- config json=$ConfigCliJson # Set AzureGitUrl
	ErrorCheck

	# Build
	echo "### Build.ps1 (Build)"
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- build
	ErrorCheck

	# Deploy
	echo "### Build.ps1 (Deploy)"
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- deploy
	ErrorCheck
}

function ErrorCheck
{
    # Check $lastexitcode and stderr
	echo "### Build.ps1 (ErrorCheck)"
	if ($lastexitcode -ne 0) { exit $lastexitcode }
	If ((Get-Content $FileNameErrorText) -ne $Null) { exit 1 }
}

function ErrorText
{
	echo "### Build.ps1 (ErrorText) - LastExitCode=$($lastexitcode)"

	If ((Get-Content $FileNameErrorText) -ne $Null) # Not equal
	{ 
		echo "### Error (Begin)"
		type $FileNameErrorText
		echo "### Error (End)"
		exit 1
	}
}

Try
{
	Main 2> $FileNameErrorText # Divert stderr to Error.txt
}
Finally
{
	ErrorText
}