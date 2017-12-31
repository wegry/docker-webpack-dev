const path = require('path')

module.exports = {
  entry: './index.js',
  output: {
    filename: 'bundle.js',
    path: path.resolve('./dist')
  },
  module: {
    rules: [{
      test: /\.scss$/,
      use: [{
        loader: "style-loader"
      }, {
        loader: "css-loader"
      }, {
        loader: "sass-loader"
      }]
    }]
  },
  plugins: [
    new require('copy-webpack-plugin')([
      { from: './index.html' }
    ])
  ],
  watchOptions: {
    poll: true
  }
};