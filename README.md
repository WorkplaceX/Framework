# Framework

![Build Status](https://github.com/WorkplaceX/ApplicationDemo/workflows/CI/badge.svg) (ApplicationDemo; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDemo.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDemo) (ApplicationDemo; travis;)

Framework to create database applications based on Angular 11 with server side rendering, ASP.NET Core 5.0 and MS-SQL. This repo contains no business logic. Company business logic typically goes into your private repo which references this framework.

## Getting started

* Get started with: [ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo) (Demo CRM and ERP system)
* Get started with: [Application](https://github.com/WorkplaceX/Application) (Empty hello world application)
* Project page: [WorkplaceX.org](http://workplacex.org)

## Project Folder Structure
* "Framework/" (Framework kernel doing all the heavy work)
* "Framework.Angular/" (Generic Angular application to render app.json sent by server)
* "Framework.Cli/" (C# Command line interface to build and deploy application)
* "Framework.Doc/" (Documentation images)
* "Framework.Test/" (Internal C# unit tests)

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

* Update Nuget packages
* Update Angular and website

* Framework.Angular/application/
```cmd
ng update
ng update --all
```

* Framework.Cli/Template/Application.Website/MasterDefault/
* Framework.Cli/Template/Application.Website/MasterEmpty/
```cmd
npm audit
npm audit fix
```

## Update Angular
```cmd
npm uninstall -g @angular/cli
npm cache clean --force
npm install -g @angular/cli
```

https://update.angular.io/
