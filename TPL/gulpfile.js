const gulp = require('gulp');

// Configuration
const config = {
  src: {
    css: 'wwwroot/css/**/*.css',
    js: 'wwwroot/js/**/*.js'
  },
  dist: {
    css: 'wwwroot/css',
    js: 'wwwroot/js',
    dist: 'wwwroot/dist'
  }
};

// Clean dist directory (using gulp built-in)
const clean = () => {
  return gulp.src(config.dist.dist, { read: false, allowEmpty: true })
    .pipe(gulp.dest('temp-delete'));
};

// Copy CSS files
const css = () => {
  return gulp.src(config.src.css)
    .pipe(gulp.dest(config.dist.css));
};

// Copy JS files
const js = () => {
  return gulp.src(config.src.js)
    .pipe(gulp.dest(config.dist.js));
};

// Watch files
const watch = () => {
  gulp.watch(config.src.css, css);
  gulp.watch(config.src.js, js);
};

// Build task
const build = gulp.series(clean, gulp.parallel(css, js));

// Default task
gulp.task('default', build);
gulp.task('clean', clean);
gulp.task('css', css);
gulp.task('js', js);
gulp.task('watch', watch);
gulp.task('build', build);
