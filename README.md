# Framework

![Build Status](https://github.com/WorkplaceX/ApplicationDemo/workflows/CI/badge.svg) (ApplicationDemo; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDemo.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDemo) (ApplicationDemo; travis;)

Framework to create database applications based on Angular 11, Bootstrap, Bulma, ASP.NET Core 5.0 and SQL Server.

Project page: [WorkplaceX.org](http://workplacex.org)

## Getting started

Create new project in empty folder (for Windows use ./cli.cmd)
```sh
# Install WorkplaceX cli into empty folder
npx workplacex-cli new

# Build everything
./cli.sh build

# Set database connection string
./cli.sh config connectionString="Data Source=localhost; Initial Catalog=ApplicationDemo; Integrated Security=True;"

# Deploy database
./cli.sh deploy

# Start application
./cli.sh start

# Open http://localhost:5000/
```

## More templates
* Get started with: [ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo) (Demo CRM and ERP system)

## Project Folder Structure
* "Framework/" (Framework kernel doing all the heavy work)
* "Framework.Angular/" (Generic Angular application to render app.json sent by server)
* "Framework.Cli/" (C# Command line interface to build and deploy application)
* "Framework.Doc/" (Documentation images)
* "Framework.Test/" (Internal C# unit tests)

## SQl-Server
Install SQL-Server for Linux or Windows: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

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
