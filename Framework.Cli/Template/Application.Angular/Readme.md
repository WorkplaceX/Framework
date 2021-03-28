# Angular 11 Client Application

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

## Add Bootstrap or Bulma
´´´cmd
npm install bootstrap --save
npm install bulma --save
´´´

File styles.scss
´´´css
@import "~bootstrap/scss/bootstrap";
@import "~bulma/css/bulma.min.css";
´´´

## Add Framework Component
```cmd
npm run ng -- generate component framework --skip-import --inlineTemplate=true --inlineStyle=true --skipTests=true
npm run ng -- generate component grid --skip-import --inlineStyle=true --skipTests=true
npm run ng -- generate component bootstrapNavbar --skip-import --inlineStyle=true --skipTests=true
npm run ng -- generate component bulma-navbar --skip-import --inlineStyle=true --skipTests=true
```

## Add Files
* frameworkStyle.scss