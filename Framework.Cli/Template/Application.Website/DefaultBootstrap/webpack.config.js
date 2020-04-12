const HtmlWebpackPlugin = require('html-webpack-plugin')
const MiniCssExtractPlugin = require('mini-css-extract-plugin'); // Prevent webpack from adding (*.scss) to (*.js) file.
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin'); // Minify (*.css) file.
var HtmlWebpackExcludeAssetsPlugin = require('html-webpack-exclude-assets-plugin'); // Prevent webpack from modifying index.html by adding (*.css).

module.exports = [{
  mode: 'development',
  context: __dirname + "/src",
  entry: './main.js',
  output: {
    filename: 'bundle.js'
  },
  module: {
    rules: [
      {
        test: /\.scss$/,
        use: [
          MiniCssExtractPlugin.loader, // creates style nodes from JS strings
          "css-loader", // translates CSS into CommonJS
          "sass-loader" // compiles Sass to CSS, using Node Sass by default
        ]
      },      

      {
        test: /\.html$/,
        use: [{
          loader: 'html-loader'
        }],
      },

      {
        test: /style.css$/,
        use: [{
          loader: 'file-loader',
          options: {name: '[path][name].[ext]'}
        }],
      },

      {
        test: /\.(png|jpg|gif|ico)$/,
        use: [{
          loader: 'file-loader',
          options: {name: '[path][name].[ext]'}
        }]
      },

      {
        test: /\bootstrap.css$/,
        use: [MiniCssExtractPlugin.loader, 'css-loader'], 
      },
  ]},

  plugins: [
    new HtmlWebpackPlugin({
      template: './index.html',
      excludeAssets: [/.css/]
    }),

    new MiniCssExtractPlugin({
      filename: "bootstrap.min.css",
    }),
  
    new HtmlWebpackExcludeAssetsPlugin(),
  ],

  optimization: {
    minimizer: [
        new OptimizeCSSAssetsPlugin(),
    ]
  },  
}];