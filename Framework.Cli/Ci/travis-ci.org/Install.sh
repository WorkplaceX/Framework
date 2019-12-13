#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

# Install .NET Core 3.1 https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Troubleshoot the package manager https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1804#troubleshoot-the-package-manager
sudo dpkg --purge packages-microsoft-prod && sudo dpkg -i packages-microsoft-prod.deb

sudo add-apt-repository universe
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1