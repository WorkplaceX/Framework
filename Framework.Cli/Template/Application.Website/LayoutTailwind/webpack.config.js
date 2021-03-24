const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin'); // Used for index.html
const MiniCssExtractPlugin = require('mini-css-extract-plugin'); // Extracts CSS into separate files

module.exports = {
  module: {
    rules: [
      // See also: https://webpack.js.org/loaders/file-loader/
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
      // See also: https://webpack.js.org/loaders/sass-loader/
      {
        test: /\.s[ac]ss$/i,
        use: [
          // Creates `style` nodes from JS strings
          "style-loader",
          // Translates CSS into CommonJS
          {
            loader: MiniCssExtractPlugin.loader,
            options: {
			  esModule: false, }, // Prevent warning: export 'default' (imported as 'content') was not found
          },
          "css-loader",
          // Compiles Sass to CSS
          "sass-loader",
        ],
      },
    ],
  },
	
  plugins: [
    new HtmlWebpackPlugin({
      template: './src/index.html', /* Input */
      filename: './index.html' /* Output */
    }),
	new MiniCssExtractPlugin(),
  ],
  
  output: {
    filename: 'main-layout.js', // Output file
	clean: true // Clear directory dist
  },
};