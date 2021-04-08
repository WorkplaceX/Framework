# Application Demo
ASP.NET Core application with Angular and MS-SQL Server.

![Build Status](https://github.com/WorkplaceX/ApplicationDemo/workflows/CI/badge.svg) (ApplicationDemo; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDemo.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDemo) (ApplicationDemo; travis;)

# Getting Started
The following components need to be installed on the machine as a prerequisite (Windows or Linux):
* [Node.js](https://nodejs.org/en/) (LTS Version)
* [.NET Core](https://dotnet.microsoft.com/download) (Version 5.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Free Express Edition)

## Install
```cmd
### Git clone (parameter recursive clones also required submodule Framework):
git clone https://github.com/WorkplaceX/ApplicationDemo.git --recursive

cd ApplicationDemo

### On first launch it will ask to register wpx command in environment path:
./wpx.cmd # For Windows
./wpx.sh # For Linux

### From now on just use:
wpx

### Set ConnectionString
wpx config connectionString="Data Source=localhost; Initial Catalog=ApplicationDemo; Integrated Security=True;" # Example Windows
wpx config connectionString="Data Source=localhost; Initial Catalog=ApplicationDemo; User Id=SA; Password=MyPassword;" # Example Linux

### Deploy Database
wpx deployDb

### Start
wpx start # http://localhost:5000/

### Stop
killall -g -SIGKILL Application.Server # Linux Only
```

## Project Folder and File Structure
* "Application/" (Application with custom business logic)
* "Application.Cli/" (Command line interface to build and deploy)
* "Application.Cli/DeployDb/" (SQL scripts to deploy to SQL server)
* "Application.Database/" (From database generated database dto objects like tables and views)
* "Application.Doc/" (Documentation images)
* "Application.Server/" (ASP.NET Core to start application)
* "Application.Website/" (Custom html and css in Angular)
* "Framework/" (External WorkplaceX framework)
* "ConfigCli.json" (Configuration file used by Application.Cli command line interface)
* "ConfigServer.json" (Generated configuration used by Application.Server web server)