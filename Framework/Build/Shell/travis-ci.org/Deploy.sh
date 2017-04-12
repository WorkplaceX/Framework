set -x
cd ./Server/bin/Debug/netcoreapp1.1/publish/
echo $(pwd)
echo $AzureGitUrl
find

git init
set +x
git remote add azure "$AzureGitUrl"
set -x
git fetch --all
git add .
git commit -m Deploy
git push azure master -f
