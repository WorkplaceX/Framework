var gulp = require('gulp');
var shell = require('gulp-shell')
var runSequence = require('run-sequence');
var es = require('event-stream');
var clean = require('gulp-clean');

// npm run gulp (../Universal/)
gulp.task('universal', shell.task([
  'cd.. & cd Universal & npm run gulp'
]))

gulp.task('clean', function () {
    return gulp.src('./Universal/')
        .pipe(clean())
})

gulp.task('cleanExpress', function () {
    return gulp.src('../UniversalExpress/Universal/')
        .pipe(clean({ force: true }))
})

// Copy file
gulp.task('copy', function () {
    return es.concat(
        gulp.src('../Universal/publish/**/*.*')
            .pipe(gulp.dest('./Universal/')),
        gulp.src('../Client/*.html')
            .pipe(gulp.dest('./Universal/')),
        gulp.src('../Client/*.css')
            .pipe(gulp.dest('./Universal/')),
        gulp.src('../Client/*.js')
            .pipe(gulp.dest('./Universal/')),
        gulp.src('../Client/dist/**/*.js')
            .pipe(gulp.dest('./Universal/dist/')),
        gulp.src('../Universal/publish/**/*.*')
            .pipe(gulp.dest('../UniversalExpress/Universal/')),
        gulp.src('../Client/node_modules/bootstrap/dist/css/bootstrap.min.css')
            .pipe(gulp.dest('../Server/wwwroot/node_modules/bootstrap/dist/css/'))
    );
})

gulp.task('default', function () {
    return runSequence('universal', 'clean', 'cleanExpress', 'copy');
});