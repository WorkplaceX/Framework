BASH_XTRACEFD=1
set -x # Print execute cammands
echo "Hello1"

function Install()
{
	echo "Hello2"
	# >&2 echo "Error2"
}

Install 2> Error.txt # stderr to Error.txt
if [ -s Error.txt ] # If Error.txt not empty
then
	echo "### Error3"
	echo "$(<Error.txt)"
	exit 1 # Exit code
fi