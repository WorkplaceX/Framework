#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

set -x # Enable print execute cammands to stdout.

FolderName=$(pwd) # Working directory

# Build Cli
cd $FolderName
cd Application.Cli
dotnet build

# Cli run build command
cd $FolderName
cd Application.Cli
dotnet run --no-build -- build
