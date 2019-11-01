# Angular 8 Client Application

## Version Check
```cmd
node --version # v12.13.0
npm --version # 6.12.0
ng --version # Angular CLI: 8.3.15
npm list -g --depth 0 # List globally installed packages
```

## Install
```cmd
ng new application
ng add @nguniversal/express-engine --clientProject application
```

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