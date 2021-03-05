# WorkplaceX Framework

![Build Status](https://github.com/WorkplaceX/ApplicationDemo/workflows/CI/badge.svg) (ApplicationDemo; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDemo.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDemo) (ApplicationDemo; travis;)

Framework to create database applications based on Angular 11, Bootstrap, Bulma, ASP.NET Core 5.0 and SQL Server. Runs on Linux and Windows. Provides CI/CD pipeline.

Project page: [WorkplaceX.org](http://workplacex.org)

## Getting started

Create new project in empty folder (for Windows use ./cli.cmd)
```sh
# Install WorkplaceX cli into an empty folder
npx workplacex-cli new

# Build everything
./cli.sh build

# Set database connection
./cli.sh config connectionString="Data Source=localhost; Initial Catalog=ApplicationDemo; User Id=SA; Password=MyPassword;"

# Deploy database
./cli.sh deploy

# Start application
./cli.sh start

# Open browser to http://localhost:5000/

# Stop server on Linux
killall -g -SIGKILL Application.Server
```

## Config
All configuration (DEV, TEST, PROD) is stored in file ConfigCli.json. Runtime configuration is automatically extracted and copied into ConfigServer.json.

## More templates
* Get started with: [ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo) (Demo CRM and ERP system)

## SQl-Server
Install SQL-Server for Linux or Windows: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

## Project Folder and File Structure
* "Application/" (Application with custom business logic in C#)
* "Application.Cli/" (Command line interface to build and deploy in C#)
* "Application.Cli/DeployDb/" (SQL scripts to deploy to SQL server)
* "Application.Database/" (From database generated C# database dto objects like tables and views)
* "Application.Doc/" (Documentation images)
* "Application.Server/" (ASP.NET Core to start application)
* "Application.Website/" (Custom html and css websites used as masters)
* "Framework/Framework/" (Framework kernel doing all the heavy work)
* "Framework/Framework.Angular/" (Generic Angular application to render app.json sent by server)
* "Framework/Framework.Cli/" (C# Command line interface to build and deploy application)
* "Framework/Framework.Doc/" (Documentation images)
* "Framework/Framework.Test/" (Internal C# unit tests)
* "ConfigCli.json" (Configuration file used by Application.Cli command line interface)
* "ConfigServer.json" (Generated configuration used by Application.Server web server)

## Version

Some versions to check:
```cmd
node --version
v12.18.1

npm --version
6.14.5

dotnet --version
5.0.101

ng --version
Angular CLI: 11.0.3
```

For Windows:
```cmd
git --version
git version 2.29.2.windows.2

$PSVersionTable.PSVersion
Major  Minor  Patch  PreReleaseLabel BuildLabel
-----  -----  -----  --------------- ----------
6      2      3
```

## Update

Checklist to update framework to latest .NET, Angular, Bootstrap, Bulma: [Update](UPDATE.md)
