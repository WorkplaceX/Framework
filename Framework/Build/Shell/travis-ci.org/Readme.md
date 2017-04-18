# Travis CI

## ConnectionString
ConnectionString in Settings Environment Variables need to be in quotation marks. Example:
* ConnectionString = "Data Source=MyDatabase.database.windows.net;User ID=MyUsername;Password=MyPassword;Initial Catalog=MyDatabase;"

## Deployment Azure Git Url
Deployment to Azure goes with Git Url. It needs to be in quotation marks. Example:
* AzureGitUrl = "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"

![alt tag](Doc/EnvironmentVariable.png)

## .travis.yml
Needs to be copied into application root folder.

## Permission
For permisson on (*.sh) files see also: http://stackoverflow.com/questions/33820638/travis-yml-gradlew-permission-denied