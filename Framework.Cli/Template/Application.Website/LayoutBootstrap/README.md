# Layout Default Website

Minimal web page with Bootstrap 4, Fontawesome 5, and Webpack 4 including dev server with hot reload.

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
* webpack, webpack-cli (Webpack)
* html-webpack-plugin, html-loader (index.html)
* file-loader (*.png)
* style-loader, css-loader, sass-loader, node-sass, postcss-loader, autoprefixer (*.scss) See also: https://getbootstrap.com/docs/4.4/getting-started/webpack/
* mini-css-extract-plugin (Extracts CSS into separate files)
* webpack-dev-server (Dev server)

