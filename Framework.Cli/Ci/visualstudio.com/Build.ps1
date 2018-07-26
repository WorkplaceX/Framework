param($AzureGitUrl, $ConnectionString)

# Init
$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"
echo 'FolderName='$FolderName

# Version
echo "### Dotnet Version"
dotnet --version