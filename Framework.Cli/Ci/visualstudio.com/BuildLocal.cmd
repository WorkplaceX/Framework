cd Framework\Framework.Cli\Ci\visualstudio.com
.\Build.ps1 -ConfigCli "{'AzureGitUrl':'https://MyUserName:MyPassword@domain.scm.azurewebsites.net:443/domain.git','WebsiteIncludeList':null}"
echo $lastexitcode
