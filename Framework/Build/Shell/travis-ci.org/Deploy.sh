set -x
cd ./Server/bin/Debug/netcoreapp1.1/publish/
echo $(pwd)
echo $AzureGitUrl
find

git init
git remote add azure "https://AzureDeploy22:Ndc73Ocd0DwQKl@framework22.scm.azurewebsites.net:443/framework22.git"
git fetch --all
git add .
git commit -m Deploy
git push azure master -f
