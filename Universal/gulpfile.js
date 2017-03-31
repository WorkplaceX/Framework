var gulp = require('gulp');
var clean = require('gulp-clean');
var rename = require("gulp-rename");
var shell = require('gulp-shell');
var runSequence = require('run-sequence');
var es = require('event-stream');
var uglify = require('gulp-uglify');
var pump = require('pump');
var fs = require('fs');

// Copy folder
gulp.task('t1', function() {
    return gulp.src('../Client/app/**/*.*')
        .pipe(gulp.dest('./src/+app/'))
})

// Rename file
gulp.task('t2', function() {
    return gulp.src('./src/+app/component.ts')
        .pipe(rename({ basename: "app.component"}))
        .pipe(gulp.dest('./src/+app/'))
})

// Delete file
gulp.task('t3', function() {
    return gulp.src('./src/+app/component.ts')
        .pipe(clean())
})

// Replace text in app.module.ts
gulp.task('t3.5', function(cb) {
    fs.readFile('../Client/app.module.ts', 'utf8', function(err, source) {
        fs.readFile('./src/+app/app.module.ts', 'utf8', function(err, dest) {
            source = source.replace(/(\r\n|\n|\r)/gm,"\r\n");
            dest = dest.replace(/(\r\n|\n|\r)/gm,"\r\n");
            /* GulpFind01 */
            indexBegin = source.indexOf('/* GulpFind01 */');
            if (indexBegin < 0) throw "Not found!";
            indexEnd = source.indexOf('\r', indexBegin);
            var replace = source.substring(indexBegin, indexEnd);
            replace = replace.replace('./app/component', './app.component');
            //
            indexBegin = dest.indexOf('/* GulpFind01 */');
            if (indexBegin < 0) throw "Not found!";
            indexEnd = dest.indexOf('\r', indexBegin);
            var find = dest.substring(indexBegin, indexEnd);
            //       
            dest = dest.replace(find, replace);     
            /* GulpFind02 */
            indexBegin = source.indexOf('/* GulpFind02 */');
            if (indexBegin < 0) throw "Not found!";
            indexEnd = source.indexOf('\r', indexBegin);
            var replace = source.substring(indexBegin, indexEnd);
            //
            indexBegin = dest.indexOf('/* GulpFind02 */');
            if (indexBegin < 0) throw "Not found!";
            indexEnd = dest.indexOf('\r', indexBegin);
            var find = dest.substring(indexBegin, indexEnd);
            //       
            dest = dest.replace(find, replace);     
            fs.writeFile('./src/+app/app.module.ts', dest, function(err){
                cb();
            })
        });
    });
})

// npm run build
gulp.task('t4', shell.task([
  'npm run build:prod:ngc'
]))

// Clean folder
gulp.task('t5', function() {
    return gulp.src('./publish/')
        .pipe(clean())
})

// Copy folder
gulp.task('t6', function() {
    return gulp.src('./dist/server/**/*.*')
        .pipe(gulp.dest('./publish/'))
})

// Copy file
gulp.task('t7', function() {
    return gulp.src('./index.html')
        .pipe(gulp.dest('./publish/src/'))
})

// Copy file
gulp.task('t8', function() {
    return pump([gulp.src('./dist/client/main.bundle.js'), uglify(), gulp.dest('./publish/')])
})



// Copy folder
gulp.task('publishIIS', function() {
    console.log('###')
    console.log('### Configure IIS to point to "C:/Temp/Publish/" and run http://localhost:8080/index.js')
    console.log('###')
    return es.concat(
        gulp.src('./publish/**/*.*')
            .pipe(gulp.dest('C:/Temp/Publish/')),
        gulp.src('./web.config')
            .pipe(gulp.dest('C:/Temp/Publish/'))
    );
})

gulp.task('default', function(){
    return runSequence('t1', 't2', 't3', 't3.5', 't4', 't5', 't6', 't7', 't8');
});