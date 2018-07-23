set -x # Enable print execute cammands to stdout.
echo "Hello"

function Main
{
    # dotnet, npm, node version check
	dotnet --version
    npm --version
    node --version
}

Main