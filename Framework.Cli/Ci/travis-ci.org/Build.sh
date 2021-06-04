#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

echo "### Build.sh"

FolderName=$(pwd)"/" # Working directory
FileNameErrorText=$FolderName"Error.txt"

BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

# Ubuntu, dotnet, node, npm, git version check
lsb_release -a
dotnet --version
node --version
npm --version
git --version

# List globally installed packages
npm list -g --depth 0

function Main
{
	# Cli build
	echo "### Build.sh (Cli Build)"
	cd $FolderName
	cd Application.Cli
	dotnet build
	ErrorCheck

	# Config
	echo "### Build.sh (Config)"
    set +x # Prevent DeployAzureGitUrl in log (PasswordHide)
    dotnet run --no-build -- config json="$ConfigCli" # Set DeployAzureGitUrl, ConnectionString ...
    set -x
	ErrorCheck

	# Build
	echo "### Build.sh (Build)"
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- build
	ErrorCheck

	# Deploy
	echo "### Build.sh (Deploy)"
	cd $FolderName
	cd Application.Cli
	dotnet run --no-build -- deploy
	ErrorCheck
}

function ErrorCheck
{
    # Check exitstatus and stderr
	echo "### Build.sh (ErrorCheck)"
	
	if [ $? != 0 ] # Exit status
	then 
		exit $? 
	fi

	if [ -s "$FileNameErrorText" ] # If Error.txt not empty
	then
		exit 1
	fi
}

function ErrorText
{
	echo "### Build.sh (ErrorText) - ExitStatus=$?"

    if [ -s "$FileNameErrorText" ] # If Error.txt not empty
	then
    	set +x # Disable print command to avoid Error.txt double in log.
	    echo "### Error (Begin)"
		echo "### Build.sh section (Cli Build, Config, Build or Deploy) wrote to STDERR. Run locally on Windows with '.\wpx.cmd build 2>Error.txt'"
	    echo "$(<$FileNameErrorText)" # Print file Error.txt 
	    echo "### Error (End)"
	    exit 1 # Set exit code
	fi
}

trap ErrorText EXIT # Run ErrorText if exception

Main 2> >(tee $FileNameErrorText) # Run main with stderr to (stdout and Error.txt).

ErrorText
