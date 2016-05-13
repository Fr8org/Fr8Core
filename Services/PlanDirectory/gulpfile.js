var gulp = require('gulp');
var bower = require('gulp-bower');

gulp.task('bower', function (done) {
    return bower({ layout: "byComponent" });
});

gulp.task('default', ['bower']);
