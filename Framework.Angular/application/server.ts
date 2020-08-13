import 'zone.js/dist/zone-node';

import { ngExpressEngine } from '@nguniversal/express-engine';
import * as express from 'express';
import { join } from 'path';

import { AppServerModule } from './src/main.server';
import { APP_BASE_HREF } from '@angular/common';
import { existsSync } from 'fs';

import * as url from 'url'; // Framework: Enable SSR POST
import * as querystring from 'querystring'; // Framework: Enable SSR POST
import * as bodyParser from 'body-parser'; // Framework: Enable SSR POST

// The Express app is exported so that it can be used by serverless Functions.
export function app(): express.Express {
  const server = express();
  
  server.use(bodyParser.json()); // Framework: Enable SSR POST 
  
  var distFolder = join(process.cwd(), 'dist/application/browser');
  
  // Framework: Enable SSR POST  
  // Running in Visual Studio
  const processCwd = process.cwd().split("\\").join("/"); // Rplace all
  if (processCwd.endsWith("Application.Server/Framework")) {
    distFolder = ".";
  } // Running on IIS
  if (processCwd.endsWith("Framework/Framework.Angular/server")) {
    distFolder = "../../"
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
	res.send("<h1>Angular Universal Server Side Rendering</h1><h2>Converts json to html. Use POST method.</h2><p>(cwd=" + process.cwd() + "; distFolder=" + distFolder + ";)</p>"); // res.render(indexHtml, { req, providers: [{ provide: APP_BASE_HREF, useValue: req.baseUrl }] }); // Framework: Enable SSR POST
  });
  
  // Framework: Enable SSR POST
  server.post('*', (req, res) => {
    let view = querystring.parse(url.parse(req.originalUrl).query).view as string;
    console.log("View=", view);
    res.render(view,     
      {
        req: req,
        res: res,
        providers: [ // See also: https://github.com/Angular-RU/angular-universal-starter/blob/master/server.ts
          {
            provide: 'jsonServerSideRendering', useValue: (req.body) // Needs app.use(bodyParser.json());
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
