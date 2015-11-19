var gulp = require('gulp');
var bower = require('gulp-bower');
var templateCache = require('gulp-angular-templatecache');

gulp.task('bower', function () {
    return bower({ layout: "byComponent" });
});

gulp.task('concattemplates', function () {
    return gulp.src('Views/AngularTemplate/**/*.cshtml')
      .pipe(templateCache('templateCache.js', {
          module: 'templates', standalone: true, transformUrl: function (url) {
              //remove .cshtml extension and /AngularTemplate/ prefix
              return '/AngularTemplate/' + url.slice(0, -7);
          }
      }))
      .pipe(gulp.dest('Scripts/tests/templates'));
});


gulp.task('default', ['bower', 'concattemplates']);