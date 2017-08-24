FolderName=$(pwd) # Working directory
BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

function Main
{
    echo \#\#\# BuildTool
    cd $FolderName
    cd BuildTool
    dotnet restore
    dotnet build
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
    dotnet run -- runSqlCreate

    # Publish Server
    echo \#\#\# Publish Server
    cd $FolderName
    cd Server
    dotnet restore
    dotnet build
    dotnet publish

    # Deploy
    Deploy
}

function Deploy
{
    cd $FolderName
    echo \#\#\# Deploy
    cd ./Server/bin/Debug/netcoreapp1.1/publish/
    echo $(pwd)
    find

    git init
    set +x # Prevent AzureGitUrl password in log
    git remote add azure "$AzureGitUrl"
    set -x
    git fetch --all 2>&1 # do not write to stderr
    git add .
    git commit -m Deploy
    git push azure master -f 2>&1 # do not write to stderr
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