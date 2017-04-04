$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.FullName + "\"
$FolderNameDotNetZip = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"
cd $FolderNameDotNetZip

# Download .NET Core 1.1 compiler
Invoke-WebRequest https://go.microsoft.com/fwlink/?linkid=843454 -OutFile dotnet.zip

# Extract zip
Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::ExtractToDirectory("$($FolderNameDotNetZip)dotnet.zip", "$($FolderNameDotNetZip)dotnet\")

cd $FolderName
cd Build
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe restore
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe build

cd $FolderName
cd Server
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe publish


