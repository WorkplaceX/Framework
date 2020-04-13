const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin') // Used for index.html
const MiniCssExtractPlugin = require('mini-css-extract-plugin'); // Extracts CSS into separate files
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = [{
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

    // See also: https://webpack.js.org/loaders/sass-loader/
      {
        test: /\.s[ac]ss$/i,
        use: [
          // Creates `style` nodes from JS strings
          'style-loader',
          MiniCssExtractPlugin.loader, // Extracts CSS into separate files. See also: https://webpack.js.org/plugins/mini-css-extract-plugin/
          // Translates CSS into CommonJS
          'css-loader',
          // Compiles Sass to CSS
          'sass-loader',
        ],
      },	  
    ]
  },

  plugins: [
    new CleanWebpackPlugin(),

    new HtmlWebpackPlugin({
      template: './src/index.html', /* Input */
      filename: './index.html' /* Output */
    }),

    new MiniCssExtractPlugin({
      filename: "main.css", /* Output */
    }),
  ],
}];