#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

function Main
{
	# Cli build
	cd $FolderName
	cd Application.Cli
	dotnet build

	# Config
    set +x # Prevent AzureGitUrl in log
    dotnet run --no-build -- config azureGitUrl="$AzureGitUrl" # Set AzureGitUrl
    set -x

	# Build
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- build

	# Deploy
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- deploy
}

set -x # Enable print execute cammands to stdout.
FolderName=$(pwd) # Working directory

cd $FolderName
Main 2> >(tee Error.txt) # Run main with stderr to stdout and Error.txt.
cd $FolderName
if [ -s Error.txt ] # If Error.txt not empty
then
    set +x # Disable print command to avoid Error.txt double in log.
	echo "### Error"
	echo "$(<Error.txt)" # Print file Error.txt 
	exit 1 # Set exit code
fi
