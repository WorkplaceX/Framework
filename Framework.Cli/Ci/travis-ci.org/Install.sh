#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

# Install .NET Core 2.2 https://www.microsoft.com/net/download/linux-package-manager/ubuntu14-04/sdk-current
yum install rh-dotnet22-dotnet-runtime-2.2 -y
scl enable rh-dotnet22 bash
