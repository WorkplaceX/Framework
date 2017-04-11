echo CS2=$ConnectionString
sudo apt-get install libunwind8
./Submodule/Framework/Build/Shell/travis-ci.org/dotnet-install.sh
dotnet --version

# npm, node version check
echo npm version:
npm --version
echo node version:
node --version

#npm update
npm install npm@latest -g

# npm, node version check
echo npm version:
npm --version
echo node version:
node --version

npm install gulp