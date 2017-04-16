set -x # Print execute cammands

function Install()
{
	echo "Hello"
}

Install 2> Error.txt # stderr to Error.txt
if [ -s Error.txt ] # If Error.txt not empty
then
	echo "### Error2"
	echo "$(<Error.txt)"
	exit 1 # Exit code
fi