# Angular 11 Client Application
Provides an empty template to start develop a web application with WorkplaceX. Add Bootstrap or Bulma as following:

## Add Bootstrap
´´´cmd
npm install bootstrap@4.6.0 --save
npm install popper.js@1.16.1 --save # Or use CDN
npm install jquery@3.5.1 --save # Or use CDN
´´´

### Modify file styles.scss
´´´css
// Your variable overrides (Bootstrap)
$body-color: #888;
$enable-rounded: false;
$enable-shadows: true;
$enable-gradients: true;
$enable-responsive-font-sizes: true;

@import "~bootstrap/scss/bootstrap";
´´´

### Modify file angular.json (Or use CDN)
´´´js
"scripts": [
    "./node_modules/jquery/dist/jquery.min.js",
    "./node_modules/popper.js/dist/umd/popper.min.js",
    "./node_modules/bootstrap/dist/js/bootstrap.min.js"              
]
´´´

### Modify file index.html to use CDN
To add scripts to index.html with CDN see https://getbootstrap.com/docs/4.6/getting-started/introduction/

## Add Bulma
´´´cmd
npm install bulma --save
´´´

Modify file styles.scss
´´´css
@import "~bulma/css/bulma.min.css";
´´´

## Version Check
´´´cmd
node --version # v12.18.1
npm --version # 6.14.5
ng --version Angular CLI: 11.2.6
npm list -g --depth 0 # List globally installed packages
´´´

## Version Check Azure
Set node version in App Service Application Settings
* WEBSITE_NODE_DEFAULT_VERSION 12.18.0
For available node versions go to App Service Advanced Tools (Kudu). Got to CMD. Type CD .. Go to D:\Program Files\nodejs

## Setup Angular CLI (Global)
´´´cmd
npm uninstall -g @angular/cli
npm cache clean --force
npm install -g @angular/cli
´´´

## Setup Angular
´´´cmd
ng new application
ng add @nguniversal/express-engine
# Test
npm start
npm run dev:ssr
ng run build:ssr
´´´

## Add Framework Component
```cmd
npm run ng -- generate component framework --skip-import --inlineTemplate=true --inlineStyle=true --skipTests=true
npm run ng -- generate component grid --skip-import --inlineStyle=true --skipTests=true
npm run ng -- generate component bootstrapNavbar --skip-import --inlineStyle=true --skipTests=true
npm run ng -- generate component bulma-navbar --skip-import --inlineStyle=true --skipTests=true
```

## Modify File package.json
Add "--outputHashing=none --progress=false"

## Modify Files
* main.ts (Find jsonServerSideRendering)
* server.ts (Find tag // Framework: Enable SSR POST)

## Add File
* frameworkStyle.scss