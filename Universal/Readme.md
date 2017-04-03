# Server Side Rendering
Based on https://github.com/angular/universal-starter (2016-12-04)

## Modifications
* File app.components.ts replace with file from folder Client.
* File app.module.ts line 11 remove "XLargeDirective".
* File app.module.ts line 15 remove "XLargeDirective".
* File server.ts line 87 change "app.get('/', ngApp);" to "app.get('*', ngApp); /*"
* File server.ts line 98 change "});" to "}); */"
* File webpack.config.ts line 82 - 89 remove.

## Publish with Gulp
* npm run gulp (Files to publish are now in folder "publish")
* npm run gulp publishIIS (Files to run IIS are now in "C:\Temp\Publish". Launch http://localhost:8080/index.js)

## Publish manually
* npm install
* npm run build
* Copy folder "src" into folder "dist/server"
* Copy folder "dist" to IIS.
* Use "web.config" with iisnode enabled.
  