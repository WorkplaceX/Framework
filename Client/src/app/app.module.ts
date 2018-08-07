import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent, Button, Selector, Grid } from './app.component';

@NgModule({
  declarations: [
    AppComponent, Button, Selector, Grid
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'Application' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
