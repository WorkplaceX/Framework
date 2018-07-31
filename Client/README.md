# Client Application with Universal

Default application has been created with

```sh
ng new Application --style=scss
```

Universal rendering has been added according https://angular.io/guide/universal

Following libraries have been added:

```sh
npm install @angular/platform-server @nguniversal/module-map-ngfactory-loader @nguniversal/express-engine --save
npm install @types/express ts-loader webpack webpack-cli --save-dev
```

Following files have been added:
* src\app\app.module.ts (Modified)
* src\app\app.server.module.ts
* src\main.server.ts
* src\tsconfig.server.json
* angular.json
* server.ts
* webpack.server.config.js

## Build Client
Following command builds client to "dist/browser" (See also: npm run build:ssr)

```sh
ng build --prod --aot --output-hashing none
```

## Build Server

Following command builds server to "dist/server"  (See also: npm run build:ssr)

```sh
ng run Application:server
```

Following command builds server to "dist/server.js"

```sh
npm run webpack:server
```

Following command starts server side rendering server

```sh
npm run serve:ssr
```

ng Generated:

# Application

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 6.0.8.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `--prod` flag for a production build.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).
