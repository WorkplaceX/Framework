const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin') // Used for index.html
const MiniCssExtractPlugin = require('mini-css-extract-plugin'); // Extracts CSS into separate files

module.exports = [{
  output: {
    filename: 'main-layout.js'
  },
  output: {
    path: path.resolve(__dirname, 'dist'), /* Output */
  },
  module: {
    rules: [
      {
        test: /\.html$/,
        use: [{
          loader: 'html-loader'
        }],
      },

      {
        test: /\.(png|jpg|gif|ico)$/,
        use: [{
          loader: 'file-loader',
          options: {
          context: 'src',
          name: '[path][name].[ext]',
        }}]
      },

      // See also: https://getbootstrap.com/docs/4.4/getting-started/webpack/
      {
        test: /\.(scss)$/,
        use: [{
          loader: 'style-loader', // inject CSS to page
        },
          MiniCssExtractPlugin.loader, { // Extracts CSS into separate files. See also: https://webpack.js.org/plugins/mini-css-extract-plugin/
          loader: 'css-loader', // translates CSS into CommonJS modules
        }, {
          loader: 'postcss-loader', // Run postcss actions
          options: {
            plugins: function () { // postcss plugins, can be exported to postcss.config.js
              return [
                require('autoprefixer')
              ];
            }
          }
        }, {
          loader: 'sass-loader' // compiles Sass to CSS
        }]
      },
    ]
  },

  plugins: [
    new HtmlWebpackPlugin({
      template: './src/index.html', /* Input */
      filename: './index.html' /* Output */
    }),

    new MiniCssExtractPlugin({
      filename: "main.css", /* Output */
    }),
  ],
}];