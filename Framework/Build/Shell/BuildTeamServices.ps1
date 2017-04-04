param($ConnectionString)

# Init
$FolderName = (get-item $PSCommandPath).Directory.Parent.Parent.Parent.Parent.FullName + "\"
$FolderNameDotNetZip = (get-item $PSCommandPath).Directory.FullName + "\"

# Download .NET Core 1.1 compiler
cd $FolderNameDotNetZip
Invoke-WebRequest https://go.microsoft.com/fwlink/?linkid=843454 -OutFile dotnet.zip

# Extract zip
Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::ExtractToDirectory("$($FolderNameDotNetZip)dotnet.zip", "$($FolderNameDotNetZip)dotnet\")

#Build
cd $FolderName
cd Build
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe restore
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe build

# ConnectionString
cd $FolderName
cd Build
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe run 01 $ConnectionString

# Server
cd $FolderName
cd Server
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe restore
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe build
cmd.exe /c $FolderNameDotNetZip\dotnet\dotnet.exe publish




