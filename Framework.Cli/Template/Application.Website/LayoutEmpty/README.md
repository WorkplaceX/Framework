# Layout Empty Website

![Build Status](https://github.com/WorkplaceX/ApplicationDemo/workflows/CI/badge.svg) (ApplicationDemo; github actions;)

[![Build Status](https://travis-ci.org/WorkplaceX/ApplicationDemo.svg?branch=master)](https://travis-ci.org/WorkplaceX/ApplicationDemo) (ApplicationDemo; travis;)

Get startet with a minimal web page configured for Webpack 4 and hot reload.

Install
```cmd
npx websitedefault
npm install
```

Start dev server (with hot reload)
```cmd
npm run start
```
Now listening on http://localhost:8080/

Build to "dist/" folder with watch:
```cmd
npm run build -- --watch
```

## Packages
* webpack webpack-cli (Webpack)
* html-webpack-plugin html-loader (index.html)
* file-loader (*.png)
* style-loader css-loader sass-loader node-sass (*.scss) See also: https://webpack.js.org/loaders/sass-loader/
* mini-css-extract-plugin (Extracts CSS into separate files)
* clean-webpack-plugin (Clean dist folder)
* webpack-dev-server (Dev server)

