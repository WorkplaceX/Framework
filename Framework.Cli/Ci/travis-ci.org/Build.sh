#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

set -x # Enable print execute cammands to stdout.

FolderName=$(pwd) # Working directory

# Cli build
cd $FolderName
cd Application.Cli
dotnet build

# Build
cd $FolderName
cd Application.Cli
dotnet run --no-build -- build

# Deploy
cd $FolderName
cd Application.Cli
dotnet run --no-build -- deploy
