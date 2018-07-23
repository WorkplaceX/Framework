#!/bin/bash
# See also: https://unix.stackexchange.com/questions/27054/bin-bash-no-such-file-or-directory

set -x # Enable print execute cammands to stdout.

echo "Hello Build"

function Main
{
    # dotnet, npm, node version check
	dotnet --version
    npm --version
    node --version
}

Main