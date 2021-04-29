import 'zone.js/dist/zone-node';

import { ngExpressEngine } from '@nguniversal/express-engine';
import * as express from 'express';
import { join } from 'path';

import { AppServerModule } from './src/main.server';
import { APP_BASE_HREF } from '@angular/common';
import { existsSync } from 'fs';

// The Express app is exported so that it can be used by serverless Functions.
export function app(): express.Express {
  const server = express();

  server.use(express.json()); // Framework: Enable SSR POST 

  var distFolder = join(process.cwd(), 'dist/application/browser');

  // Framework: Enable SSR POST
  var mode = "Console";
  var folderName = join(process.cwd(), '../').split("\\").join("/"); // Rplace all
  if (folderName.endsWith("Application.Server/Framework/Application.Website/")) {
    // Running in Visual Studio
    mode = "Visual Studio"
    distFolder = join(process.cwd(), '../browser/');
  } else {
    folderName = join(process.cwd(), '../../').split("\\").join("/"); // Rplace all
    if (folderName.endsWith("Framework/Application.Website/")) {
      // Running on IIS
      mode = "IIS";
      distFolder = join(process.cwd(), '../browser/');
    }
  }

  const indexHtml = existsSync(join(distFolder, 'index.original.html')) ? 'index.original.html' : 'index';

  // Our Universal express-engine (found @ https://github.com/angular/universal/tree/master/modules/express-engine)
  server.engine('html', ngExpressEngine({
    bootstrap: AppServerModule,
  }));

  server.set('view engine', 'html');
  server.set('views', distFolder);

  // Example Express Rest API endpoints
  // server.get('/api/**', (req, res) => { });
  // Serve static files from /browser
  server.get('*.*', express.static(distFolder, {
    maxAge: '1y'
  }));

  // All regular routes use the Universal engine
  server.get('*', (req, res) => {
    // res.render(indexHtml, { req, providers: [{ provide: APP_BASE_HREF, useValue: req.baseUrl }] }); // Framework: Enable SSR POST
    res.send(
      "<h1>Angular Universal Server Side Rendering</h1><h2>Converts json to html. Use POST method.</h2>" + 
      "<p>" + 
      "mode=" + mode + ";<br />" +
      "cwd=" + process.cwd() + ";<br />" + 
      "distFolder=" + distFolder + ";<br />" + 
      "</p>"
      ); 
  });

  // Framework: Enable SSR POST
  server.post('*', (req, res) => {
    console.log("Render (SSR)");
    res.render(indexHtml,     
      {
        req: req,
        res: res,
        providers: [ // See also: https://github.com/Angular-RU/angular-universal-starter/blob/master/server.ts
          {
            provide: 'jsonServerSideRendering', useValue: (req.body) // Needs server.use(express.json());
          }
        ]
      },
    );
  });

  return server;
}

function run(): void {
  const port = process.env.PORT || 4000;

  // Start up the Node server
  const server = app();
  server.listen(port, () => {
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

// Webpack will replace 'require' with '__webpack_require__'
// '__non_webpack_require__' is a proxy to Node 'require'
// The below code is to ensure that the server is run only when not requiring the bundle.
declare const __non_webpack_require__: NodeRequire;
const mainModule = __non_webpack_require__.main;
const moduleFilename = mainModule && mainModule.filename || '';
if (moduleFilename === __filename || moduleFilename.includes('iisnode')) {
  run();
}

export * from './src/main.server';
