# .travis.yml
File needs to be copied from here into application root folder. That's where travis is looking for it.

## Config Azure Git Url
Deployment to Azure goes via Git Url. In travis settings parameter needs to be in quotation marks. Example:
* AzureGitUrl = "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"

On Azure portal go to
* Properties (To get git url)
* Deployment credentials (To set git url password)

## Permission
For permission on (*.sh) files see also: http://stackoverflow.com/questions/33820638/travis-yml-gradlew-permission-denied. Example: "git update-index --chmod=+x Install.sh"