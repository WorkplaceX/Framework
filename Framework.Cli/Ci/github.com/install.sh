BASH_XTRACEFD=1 # Print execute command to stdout. Not to stderr.
set -x # Enable print execute cammands to stdout.

# Ubuntu, dotnet, node, npm version check
lsb_release -a
dotnet --version
node --version
npm --version

# List globally installed packages
npm list -g --depth 0

# Install Git
sudo apt update
sudo apt install git
git --version
