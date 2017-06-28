# Framework
Framework to create applications based on Angular 4 with server side rendering, ASP.NET Core and MS-SQL. This repo contains no business logic. Business logic goes into your repo like this:
* [ApplicationDemo](https://github.com/WorkplaceX/ApplicationDemo)

## Folder Structure
* **Application/** (Contains .NET Core business logic)
* **BuildTool/** (Build and deploy scripts with command line interface CLI. Also used for CI)
* **Server/** (ASP.NET Core server)
* **Submodule/Client/** (Angular 4 client)
* **Submodule/Framework/** (Framework with generic functions)
* **Submodule/Office/** (ETL to load (*.xlsx) files into database. It's ".NET". Not ".NET Core"!)
* **Submodule/UniversalExpress/** (Used for debug in Visual Studio only. Runs Universal in Express server)

## Install
* cd BuildTool
* dotnet run -- -h (To show all CLI commands)
* dotnet run -- connection [ConnectionString] (Set ConnectionString)
* dotnet run -- connection --check (Check ConnectionString)
* dotnet run -- installAll
* dotnet run -- serve

## BuildTool CLI
Command line interface containing all scripts and tools to build and deploy. Also used for CI.

## Visual Studio 2017
* Install the following VS2017 components:

![alt tag](Framework.BuildTool/Doc/VisualStudioPrerequisite.png)

* Open Application.sln

* Configure multiple start up projects (Server, UniversalExpress)

![alt tag](Framework.BuildTool/Doc/VisualStudioStartup.png)

