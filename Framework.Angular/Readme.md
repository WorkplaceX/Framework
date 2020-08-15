# Angular 10 Client Application

## Version Check
```cmd
node --version # v12.13.0
npm --version # 6.12.0
ng --version # Angular Angular CLI: 10.0.6
npm run ng -- --version # Angular CLI: 10.0.6 run if no global Angular is installed. Run in folder Framework/Framework.Angular/application/
npm list -g --depth 0 # List globally installed packages
```

## Setup Init
Delete folder Framework/Framework.Angular/application/
Push to new branch

## Setup Angular CLI
Run in folder Framework/Framework.Angular
```cmd
npm install @angular/cli
```

## Create package.json for Angular CLI
In folder Framework/Framework.Angular/
```json
{
  "scripts": {
	"ng": "ng"
  }
}
```

## Setup Angular
Create new application with Angular CLI.
```cmd
npm run ng -- new application
```

Delete files and folders of initial Angular CLI
* node_modules
* package.json
* package-lock.json

```cmd
cd application
npm run ng -- add @nguniversal/express-engine
```

## Modify file package.json
Add "--outputHashing=none --progress=false"

## Modify file server.ts
See source code tag: "// Framework: Enable SSR POST"

## Build and Start
```cmd
npm run build:ssr # Output dist/ folder
npm run start
npm run serve:ssr # Use POST method
```

## Add Data Service
```cmd
npm run ng -- generate service data
```

* Add HttpClient

## Add Framework Component
```cmd
npm run ng -- generate component framework --skip-import --inlineTemplate=true --inlineStyle=true --skipTests=true
npm run ng -- generate component grid --skip-import --inlineStyle=true --skipTests=true
npm run ng -- generate component bootstrapNavbar --skip-import --inlineStyle=true --skipTests=true
```

## Application.Website
* Create new folder src/Application.Website (Run later cli build to populate)
* Add it to .gitignore /src/Application.Website
### Modify angular.json
* Replace "sourceRoot": "src", with "sourceRoot": "src/Application.Website/Default",
* Replace "index": "src/index.html", with "index": "src/Application.Website/Default/index.html",
* Remove line "src/favicon.ico",
* Replace "src/assets" with "src/Application.Website/Default"

## Server Side Rendering (Universal)
### Running in Angular Environment
Start with npm run serve:ssr
* http://localhost:4000 (GET)
* cwd=Framework\Framework.Angular\application
### Running in Visual Studio
When running in IIS Express or as Application external node is started. See also method StartUniversalServer();
* http://localhost:4000/ (GET)
* http://localhost:4000/?view=Application.Website%2fDefault%2findex.html (POST)
* cwd=Application.Server\Framework
### Running on IIS
* http://localhost:8080/Framework/Framework.Angular/server/main.js (GET)
* http://localhost:8080/Framework/Framework.Angular/server/main.js?view=Application.Website%2fDefault%2findex.html (POST)
* cwd=Framework\Framework.Angular\server

# Run on local IIS Server
In order to run application on local IIS server install:
* [Hosting Bundle for Windows](https://dotnet.microsoft.com/download/thank-you/dotnet-runtime-3.0.0-windows-hosting-bundle-installer) Enable .NET Core hosting on IIS.
* [iisnode-full-v0.2.21-x64.msi](https://github.com/azure/iisnode) Enable nodejs on IIS server.
