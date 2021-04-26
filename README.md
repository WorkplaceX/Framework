# WorkplaceX Framework

![Build Status](https://github.com/WorkplaceX/ApplicationDoc/workflows/CI/badge.svg) (ApplicationDoc; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDoc.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDoc) (ApplicationDoc; travis;)

Framework to create database applications based on Angular 11, Bootstrap, Bulma, ASP.NET Core 5.0 and SQL Server. Runs on Windows and Linux. Provides CI/CD pipeline.

Project page: [WorkplaceX.org](https://www.workplacex.org)

## Getting started
Prerequisites for Linux and Windows
* [Node.js](https://nodejs.org/en/) (LTS Version)
* [.NET Core SDK](https://dotnet.microsoft.com/download) (Version 5.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Free Express)

Create new project in empty folder (for Linux use ./cli.sh instead of /.cli.cmd)
```cmd
# Install WorkplaceX cli into an empty folder
npx workplacex-cli new

# Build everything
./cli.cmd build

# Set database connection
./cli.cmd config connectionString="Data Source=localhost; Initial Catalog=ApplicationDoc; User Id=SA; Password=MyPassword;"

# Deploy database
./cli.cmd deployDb

# Start application
./cli.cmd start

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
