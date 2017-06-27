import 'reflect-metadata';
import 'zone.js/dist/zone-node';
import { platformServer, renderModuleFactory } from '@angular/platform-server'
import { enableProdMode } from '@angular/core'
import { AppServerModuleNgFactory } from '../dist/ngfactory/src/app/app.server.module.ngfactory'
import * as express from 'express';
import { readFileSync } from 'fs';
import { join } from 'path';
import * as bodyParser from 'body-parser';

const PORT = process.env.PORT || 4000;

enableProdMode();

const app = express();

let template = readFileSync(join(__dirname, '..', 'src', 'index.html')).toString();

app.use(bodyParser.json());

app.engine('html', (_, options, callback) => {
  const opts = { 
    document: template, 
    url: options.req.url, 
    extraProviders: [{ provide: 'requestBodyJson', useFactory: () => options.req.body }] // Needs: app.use(bodyParser.json()); 
 };

  renderModuleFactory(AppServerModuleNgFactory, opts)
    .then(html => callback(null, html));
});

app.set('view engine', 'html');
app.set('views', 'src')

// app.get('*.*', express.static(join(__dirname, '..', 'dist')));

app.get('/Universal/index.js', (req, res) => {
  res.render('index', { req });
});

app.post('/Universal/index.js', (req, res) => {
  res.render('index', { req });
});

app.listen(PORT, () => {
  console.log(`listening on http://localhost:${PORT}/Universal/index.js!`);
});