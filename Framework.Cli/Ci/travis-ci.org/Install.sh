#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

# Install .NET Core 2.1 https://www.microsoft.com/net/download/linux-package-manager/ubuntu14-04/sdk-current
wget -q https://packages.microsoft.com/config/ubuntu/14.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1

# dotnet, npm, node version check
dotnet --version
npm --version
node --version
