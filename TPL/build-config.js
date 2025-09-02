// Build Configuration for TPL Web Application
module.exports = {
  // Webpack configuration
  webpack: {
    entry: {
      app: './src/js/app.js',
      chat: './src/js/chat.js',
      vendor: './src/js/vendor.js'
    },
    output: {
      path: './wwwroot/dist',
      filename: '[name].[contenthash].js',
      clean: true
    }
  },
  
  // Bundle configuration
  bundle: {
    css: {
      input: './src/css/**/*.css',
      output: './wwwroot/css/bundle.min.css'
    },
    js: {
      input: './wwwroot/js/**/*.js',
      output: './wwwroot/js/bundle.min.js'
    }
  },
  
  // PostCSS configuration
  postcss: {
    plugins: [
      require('autoprefixer'),
      require('cssnano')
    ]
  },
  
  // Babel configuration
  babel: {
    presets: ['@babel/preset-env']
  }
};

