FolderName=$(pwd) # Working directory
BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

function Main
{
	# BuildTool InstallAll
    echo \#\#\# BuildTool InstallAll
    cd $FolderName
    cd BuildTool
    set +x # Prevent ConnectionString in log
    dotnet run -- connection "$ConnectionString" # Set ConnectionString
    set -x
    dotnet run --no-build -- installAll
	ErrorCheck

    # BuildTool UnitTest
    echo \#\#\# BuildTool UnitTest
    cd $FolderName
    cd BuildTool
	dotnet run --no-build -- unitTest
	ErrorCheck

    # BuildTool RunSqlCreate 
    echo \#\#\# BuildTool RunSqlCreate 
    cd $FolderName
    cd BuildTool
    # dotnet run --no-build -- runSqlCreate # Run sql update manually from BuildTool CLI because of database firewall.
	ErrorCheck

    # BuildTool Deploy
    echo \#\#\# BuildTool Deploy
    cd $FolderName
    cd BuildTool
    set +x # Prevent AzureGitUrl password in log
    dotnet run --no-build -- deploy "$AzureGitUrl" # publish
    set -x
	ErrorCheck
}

function ErrorCheck
{
	cd $FolderName
	if [ -s Error.txt ] # If Error.txt not empty
	then
		set +x # Disable print command to avoid Error.txt double in log.
		echo "### Error"
		echo "$(<Error.txt)" # Print file Error.txt 
		exit 1 # Set exit code
	fi
}

cd $FolderName
Main 2> >(tee Error.txt) # stderr to stdout and Error.txt.
