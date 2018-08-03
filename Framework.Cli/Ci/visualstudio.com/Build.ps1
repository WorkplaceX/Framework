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
    dotnet run --no-build -- config json="$ConfigCliJson" # Set AzureGitUrl
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
    # Check $lastexitcode and stderr to exit
	echo "### Build.ps1 (ErrorCheck)"
	if ($lastexitcode -ne 0) 
    { 
    	echo "### Build.ps1 (ErrorCheck) - LastExitCode=$($lastexitcode)"
        exit $lastexitcode 
    }

	If ((Get-Content $FileNameErrorText) -ne $Null) 
    { 
    	echo "### Build.ps1 (ErrorCheck) - ErrorText"
        exit 0 
    }
}

function ErrorText
{
	echo "### Build.ps1 (ErrorText)"

	If ((Get-Content $FileNameErrorText) -ne $Null) # Not equal
	{ 
		echo "### Build.ps1 (ErrorText) - Begin"
		type $FileNameErrorText
		echo "### Build.ps1 (ErrorText) - End"
		exit 1
	}
	else
	{
		echo "### Build.ps1 (ErrorText) - No Error"
	}
}

Try
{
	echo "### Build.ps1 (Try)"
	Main 2> $FileNameErrorText # Divert stderr to Error.txt
}
Finally
{
	echo "### Build.ps1 (Finally) - LastExitCode=$($lastexitcode)"
	ErrorText
}
echo "### Build.ps1 (LastExitCode=$($lastexitcode))"
