BASH_XTRACEFD=1 # Execute command to stdout. Not stderr.
set -x # Enable print execute cammands to stdout.

function Build
{
    FolderName=$(pwd)

    echo \#\#\# Build
    cd $FolderName
    cd Build
    dotnet restore
    dotnet build
    # Set ConnectionString
    set +x
    dotnet run 01 "$ConnectionString" 
    set -x
    # InstallAll
    dotnet run 02 

	
    cd $FolderName
}

function Deploy
{
    set -x
    cd ./Server/bin/Debug/netcoreapp1.1/publish/
    echo $(pwd)
    find

    git init
    set +x
    git remote add azure "$AzureGitUrl"
    set -x
    git fetch --all
    git add .
    git commit -m Deploy
    git push azure master -f
}

Build 2>&1 Error.txt # stderr to Error.txt
if [ -s Error.txt ] # If Error.txt not empty
then
	echo "### Error"
	echo "$(<Error.txt)" # Print file Error.txt 
	exit 1 # Set exit code
fi