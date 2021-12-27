# WorkplaceX Framework

![Build Status](https://github.com/WorkplaceX/ApplicationDoc/workflows/CI/badge.svg) (github actions)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDoc.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDoc) (travis)

Framework to create database applications based on Angular 11, Bootstrap, Bulma, ASP.NET Core 6.0 and SQL Server. Runs on Windows and Linux. Provides CI/CD pipeline.

Project page: [WorkplaceX.org](https://www.workplacex.org)

## Getting started
Prerequisites for Linux and Windows
* [Node.js](https://nodejs.org/en/) (LTS Version)
* [.NET Core SDK](https://dotnet.microsoft.com/download) (Version 6.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Free Express)

Create new project in empty folder (for Linux use ./cli.sh instead of /.cli.cmd)
```cmd
# Install WorkplaceX cli into an empty folder
npx workplacex-cli new

# Build everything
./wpx.cmd build

# Set database connection
./wpx.cmd config connectionString="Data Source=localhost; Initial Catalog=ApplicationDoc; User Id=SA; Password=MyPassword;"

# Deploy database
./wpx.cmd deployDb

# Start application
./wpx.cmd start

# Open browser to http://localhost:5000/

# Stop server on Linux
killall -g -SIGKILL Application.Server
```

## Config
All configuration (DEV, TEST, PROD) is stored in file ConfigCli.json. Runtime configuration is automatically extracted and copied into ConfigServer.json.

## More templates
* Get started with: [ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo) (Demo CRM and ERP system)

## Project Folder and File Structure
* "Application/" (Application with custom business logic in C#)
* "Application.Cli/" (Command line interface to build and deploy in C#)
* "Application.Cli/DeployDb/" (SQL scripts to deploy to SQL server)
* "Application.Database/" (From database generated C# database dto objects like tables and views)
* "Application.Doc/" (Documentation images)
* "Application.Server/" (ASP.NET Core to start application)
* "Application.Website/" (Angular application)
* "Framework/Framework/" (Framework kernel doing all the heavy work)
* "Framework/Framework.Cli/" (C# Command line interface to build and deploy application)
* "Framework/Framework.Doc/" (Documentation images)
* "Framework/Framework.Test/" (Internal C# unit tests)
* "ConfigCli.json" (Configuration file used by Application.Cli command line interface)
* "ConfigServer.json" (Generated configuration used by Application.Server web server)

## Version

Some versions to check:
```cmd
node --version
v16.13.1

npm --version
8.1.2

dotnet --version
6.0.101

ng --version
Angular CLI: 13.1.2
```

For Windows:
```cmd
git --version
git version 2.34.1.windows.1

$PSVersionTable.PSVersion
Major  Minor  Patch  PreReleaseLabel BuildLabel
-----  -----  -----  --------------- ----------
6      2      3
```

## Update

Checklist to update framework to latest .NET, Angular, Bootstrap, Bulma: [Update](UPDATE.md)
