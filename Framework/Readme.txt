Release
-Disable file name hashing on client build.
-Json serialize debug remove.
-Remove field TypeCSharp, method Component.TypeSet(); --> No, support derive from Component. [Done]

Release b1.0
-Remove Component.Constructor text parameter.
-Split SqlName attribute into SqlTableName and SqlColumnName.
-New location of Application.json.
-Rename Database.lock.cs to Database.g.cs [Done]
-Angular Selector IsHide on all removeSelector [Done]
-Class Page [Done]
-Tool CLI [Done]
-Remove Build [Done]
-Remove Office [Done]
-FormatterServices.GetUninitializedObject. See also json package code.

Tool
-"C:\Program Files (x86)\Google\Chrome\Application\chrome" --disable-web-security --user-data-dir
-https://coursetro.com/posts/code/68/Make-your-Angular-App-SEO-Friendly-(Angular-4-+-Universal)
-http://www.xiconeditor.com/ (Online ico editor)
-https://pixabay.com/ (Images)

ToDo
-Include build number into version https://docs.travis-ci.com/user/environment-variables/
-https://github.com/azure/iisnode/wiki/iisnode-releases, https://www.youtube.com/watch?v=JUYCDnqR8p0 (Document install iisnode)
-Rename Cell.V to Cell.T (Text) [Done]
-Include SchemaName into TableName [Done]
-Rename GridCell.IsSelect to IsFocus
-Focused field based on expression not on GridCell.IsSelect. Same for Row.IsSelect. Be aware of trippel select state with mouse over.
-https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/ (Open Browser)
-https://unpkg.com/@angular/core@2.1.2/bundles/core.umd.js
-JsonComponent.Parent
-SQL Select async
-IIS connect to LocalDb [Done]
-Special characters in data. [Done]
-node.module.ts line 76; dataService.ts line 27; pass json object directly. Not stringify and then parse again!
-https://varvy.com/pagespeed/defer-loading-javascript.html [Done]
-Transport data object on first request.
-https://scotch.io/tutorials/all-the-ways-to-add-css-to-angular-2-components (Font)

Tools
-https://developer.microsoft.com/en-us/microsoft-edge/tools/screenshots/

### client.aot.ts
require('core-js/shim');
require('zone.js');
require('reflect');
var SystemJS = require('systemjs');
SystemJS.import('https://unpkg.com/@angular/core@2.1.2/bundles/core.umd.js').then(function (m) {
  console.log("M=" + m);
});


### webpack.config.ts
export var clientConfig = {
  target: 'web',
  entry: './src/client',
  output: {
    path: root('dist/client')
  },
  node: {
    global: true,
    crypto: 'empty',
    __dirname: true,
    __filename: true,
    process: true,
    Buffer: false
  },
  externals: {
    "@angular/core": "angular9"
  }
};
