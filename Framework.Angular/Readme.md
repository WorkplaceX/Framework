# Angular 8 Client Application

## Version Check
```cmd
node --version # v12.13.0
npm --version # 6.12.0
ng --version # Angular CLI: 8.3.15
npm list -g --depth 0 # List globally installed packages
```

## Setup Angular
```cmd
ng new application
ng add @nguniversal/express-engine --clientProject application
```

## Modify file package.json
Add "--outputHashing=none" and "--progress=false" to npm script "build:client-and-server-bundles"

## Modify file server.ts
See tag: "// Framework: Enable SSR POST"

## Build and Start
```cmd
npm run build:client-and-server-bundles
npm run start
```

## Add Data Service
```cmd
ng generate service data
```

* Add HttpClient