# Framework

![Build Status](https://github.com/WorkplaceX/ApplicationDemo/workflows/CI/badge.svg) (ApplicationDemo; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDemo.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDemo) (ApplicationDemo; travis;)

Framework to create database applications based on Angular 6 with server side rendering, ASP.NET Core 2.2 and MS-SQL. This repo contains no business logic. Company business logic typically goes into your private repo which references this framework.

## Getting started

* Get started with: [ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo) (Demo CRM and ERP system)
* Get started with: [Application](https://github.com/WorkplaceX/Application) (Empty hello world application)
* Documentation see: [Documentation](https://github.com/WorkplaceX/Framework/wiki) (wiki)
* Project page: [WorkplaceX.com](http://workplacex.com)

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
v12.13.0

npm --version
6.12.0

dotnet --version
3.1.101

ng --version
Angular CLI: 8.3.15
```

For Windows:
```cmd
git --version
git version 2.21.0.windows.1

$PSVersionTable.PSVersion
Major  Minor  Patch  PreReleaseLabel BuildLabel
-----  -----  -----  --------------- ----------
6      2      3
```
