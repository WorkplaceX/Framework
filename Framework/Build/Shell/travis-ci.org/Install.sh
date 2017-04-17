FolderName=$(pwd) # Working directory
BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

# https://www.microsoft.com/net/core#linuxubuntu
sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
sudo apt-get update
sudo apt-get install dotnet-dev-1.0.1

function Main
{
    dotnet --version

    # npm, node version check
    npm --version
    node --version

    #npm update
    npm install npm@latest --loglevel error

    # npm, node version check
    npm --version
    node --version

    npm install gulp --loglevel error
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