set -x # Print execute cammands

function Install()
{
    # https://www.microsoft.com/net/core#linuxubuntu
    sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ trusty main" > /etc/apt/sources.list.d/dotnetdev.list'
    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
    sudo apt-get update
    sudo apt-get install dotnet-dev-1.0.1

    dotnet --version

    # npm, node version check
    npm --version
    node --version

    #npm update
    npm install npm@latest -g

    # npm, node version check
    npm --version
    node --version

    npm install gulp
}

Install 2> Error.txt # stderr to Error.txt
if [ -s Error.txt ] # If Error.txt not empty
then
	echo "### Error"
	echo "$(<Error.txt)"
	exit 1 # Exit code
fi