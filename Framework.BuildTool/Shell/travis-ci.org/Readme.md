# Travis CI

## ConnectionString
ConnectionString in Settings Environment Variables need to be in quotation marks. Example:
* ConnectionString = "Data Source=MyDatabase.database.windows.net;User ID=MyUsername;Password=MyPassword;Initial Catalog=MyDatabase;"

## Deployment Azure Git Url
Deployment to Azure goes with Git Url. It needs to be in quotation marks. Example:
* AzureGitUrl = "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"

On Azure portal go to "App Service" --> "Deployment options" --> select "Local Git Repository" click select. On "App Service" --> "Properties" the "GIT URL" is shown. 

**Add ":MyPassword" to it!**

![TravisCI](https://github.com/WorkplaceX/Framework/blob/master/Doc/TravisEnvironment.png)

## .travis.yml
Needs to be copied into application root folder. That's where travis is looking for the file.

## Permission
For permission on (*.sh) files see also: http://stackoverflow.com/questions/33820638/travis-yml-gradlew-permission-denied. Example: "git update-index --chmod=+x Install.sh"

## SQL Server Firewall
By default the command "BuildTool runSqlCreate" is disabled because of the database firewall. Run it manually on a machine you have database access.
