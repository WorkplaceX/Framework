import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent, Button, Div, Selector, Grid, Page, Html } from './app.component';
import { BootstrapNavbar } from './bootstrapNavbar/bootstrapNavbar.component';

@NgModule({
  declarations: [
    AppComponent, Button, Div, BootstrapNavbar, Selector, Grid, Page, Html
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'Application' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
