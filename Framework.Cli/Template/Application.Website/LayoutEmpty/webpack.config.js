const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = [{
  module: {
    rules: [
      // See also: https://webpack.js.org/loaders/file-loader/#root
      {
        test: /\.(png|jpe?g|gif|ico)$/i,
        use: [
          {
            loader: 'file-loader',
            options: {
              context: 'src',
              name: '[path][name].[ext]',
            }
          },
        ],
      },

      // See also: https://webpack.js.org/loaders/sass-loader/#getting-started, https://webpack.js.org/plugins/mini-css-extract-plugin/
      {
        test: /\.s[ac]ss$/i,
        use: [
          // Creates `style` nodes from JS strings
          // "style-loader", // Replaced by MiniCssExtractPlugin. Prevent warning: export 'default' (imported as 'content') was not found
          // Translates CSS into CommonJS
          MiniCssExtractPlugin.loader,
          "css-loader",
          // Compiles Sass to CSS
          "sass-loader",
        ],
      },    
    ]
  },

  plugins: [
    new HtmlWebpackPlugin({
      template: './src/index.html', // Input html file
      filename: './index.html' // Output html file
    }),
    new MiniCssExtractPlugin(),
  ],

  output: {
    filename: 'main-layout.js', // Output js file
    clean: true // Clear directory dist
  },
}];