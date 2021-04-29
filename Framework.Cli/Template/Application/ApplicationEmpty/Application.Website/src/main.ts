import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

if (environment.production) {
  enableProdMode();
}

document.addEventListener('DOMContentLoaded', () => {
  platformBrowserDynamic(
    [
      { 
        provide: 'jsonServerSideRendering', useValue: null // Default value for data.service.ts when running normal without server side rendering. Make it injectable.
      }
    ]    
  ).bootstrapModule(AppModule)
  .catch(err => console.error(err));
});
