FolderName=$(pwd) # Working directory
BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

function Main
{
    echo \#\#\# BuildTool
    cd $FolderName
    cd BuildTool
    # Set ConnectionString
    set +x # Prevent ConnectionString in log
    dotnet run -- connection "$ConnectionString" 
    set -x
    # InstallAll
    echo \#\#\# InstallAll
    dotnet run -- installAll

    # BuildTool runSqlCreate 
    echo \#\#\# RunSqlCreate
    cd $FolderName
    cd BuildTool
    # dotnet run -- runSqlCreate # Run sql update manually from BuildTool CLI because of database firewall.

    # Build Server
    echo \#\#\# Build Server
    cd $FolderName
    cd Server
    dotnet restore
    dotnet build

    # Publish and Deploy Server
    echo \#\#\# Publish and Deploy Server
    cd $FolderName
    cd BuildTool
    set +x # Prevent AzureGitUrl password in log
    dotnet run -- deploy "$AzureGitUrl"
    set -x
}

cd $FolderName
Main 2> >(tee Error.txt) # stderr to stdout and Error.txt.

cd $FolderName
if [ -s Error.txt ] # If Error.txt not empty
then
    set +x # Disable print command to avoid Error.txt double in log.
	echo "### Error"
	echo "$(<Error.txt)" # Print file Error.txt 
	exit 1 # Set exit code
fi