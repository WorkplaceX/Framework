# Angular 9 Client Application

## Version Check
```cmd
node --version # v12.13.0
npm --version # 6.12.0
ng --version # Angular CLI: 9.1.1
npm list -g --depth 0 # List globally installed packages
```

## Setup Angular
Create new application with Angular CLI.
```cmd
ng new application
ng add @nguniversal/express-engine
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
ng generate service data
```

## Add Framework Component
```cmd
ng generate component framework --skip-import --inlineTemplate=true --inlineStyle=true --skipTests=true
ng generate component grid --skip-import --inlineStyle=true --skipTests=true
ng generate component bootstrapNavbar --skip-import --inlineStyle=true --skipTests=true
```

## Add Framework Style
Add new file frameworkStyle.scss and link it in styles.scss with @import "frameworkStyle"

* Add HttpClient

# Run on local IIS Server
In order to run application on local IIS server install:
* [Hosting Bundle for Windows](https://dotnet.microsoft.com/download/thank-you/dotnet-runtime-3.0.0-windows-hosting-bundle-installer) Enable .NET Core hosting on IIS.
* [iisnode-full-v0.2.21-x64.msi](https://github.com/azure/iisnode) Enable nodejs on IIS server.