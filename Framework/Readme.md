# Framework 2019-03-27

Application framework library.

## Ubuntu 18.04 on Windows 10

Install Ubuntu 18.04 on Windows 10 from Microsoft Store. (See also: https://www.microsoft.com/en-us/p/ubuntu-1804/9n9tngvndl3q)

Install Node (See also: https://nodejs.org/en/download/package-manager/)
```sh
curl -sL https://deb.nodesource.com/setup_8.x | sudo -E bash -
sudo apt-get install -y nodejs
```

Install .NET Core (See also: https://www.microsoft.com/net/download/linux-package-manager/ubuntu18-04/sdk-current)
```sh
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

Install Git (See also: https://git-scm.com/download/linux)
```sh
apt-get install git
```


