import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent, Button, Selector } from './app.component';

@NgModule({
  declarations: [
    AppComponent, Button, Selector
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'Application' }),
    HttpClientModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
