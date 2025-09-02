const path = require('path');

module.exports = {
  entry: {
    app: './wwwroot/js/helpers.js',
    menu: './wwwroot/js/menu.js',
    template: './wwwroot/js/template-customizer.js',
    dropdown: './wwwroot/js/dropdown-hover.js',
    mega: './wwwroot/js/mega-dropdown.js',
    persian: './wwwroot/js/persian-date.min.js',
    persianPicker: './wwwroot/js/persian-datepicker.min.js',
    runtime: './wwwroot/js/runtime.js',
    vendors: './wwwroot/js/vendors.js'
  },
  output: {
    path: path.resolve(__dirname, 'wwwroot/dist'),
    filename: '[name].bundle.js',
    clean: true
  },
  mode: 'development',
  devtool: 'source-map'
};
