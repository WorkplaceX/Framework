#!/bin/bash
set -x # Enable print execute cammands to stdout.
FolderName=$(pwd) # Working directory

# Install .NET Core 2.1 https://www.microsoft.com/net/download/linux-package-manager/ubuntu14-04/sdk-current
wget -q https://packages.microsoft.com/config/ubuntu/14.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1

echo "Hello"
echo $FolderName

function Main
{
    # dotnet, npm, node version check
	dotnet --version
    npm --version
    node --version
}

Main