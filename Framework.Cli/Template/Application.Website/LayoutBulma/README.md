# Layout Bulma Website

Minimal web page based on Layout Empty Website and Bulma.

See also: https://bulma.io/documentation/customize/with-webpack/

Install
```cmd
npm install
```

Build to "dist/" folder with watch:
```cmd
npm run build -- --watch
```

Start dev server (with hot reload)
```cmd
npm run start
```

## Packages
* webpack webpack-cli (Webpack)
* html-webpack-plugin html-loader (index.html)
* file-loader (*.png)
* style-loader css-loader sass-loader node-sass (*.scss) See also: https://webpack.js.org/loaders/sass-loader/
* mini-css-extract-plugin (Extracts CSS into separate files)
* clean-webpack-plugin (Clean dist folder)
* webpack-dev-server (Dev server)

