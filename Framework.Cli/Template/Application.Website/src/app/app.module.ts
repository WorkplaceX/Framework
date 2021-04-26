import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { Page, Selector, Button, Html, Div, DivContainer, BingMap, Dialpad } from './framework/framework.component';
import { BulmaNavbarComponent } from './bulma-navbar/bulma-navbar.component';
import { GridComponent } from './grid/grid.component';
import { BootstrapNavbarComponent } from './bootstrap-navbar/bootstrap-navbar.component';
import { Custom01Component } from './custom01/custom01.component';

import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Custom02Component } from './custom02/custom02.component';
import { Custom03Component } from './custom03/custom03.component';

@NgModule({
  declarations: [
    AppComponent, Selector, Page, Button, Html, Div, DivContainer, BingMap, Dialpad, BulmaNavbarComponent, GridComponent, BootstrapNavbarComponent, Custom01Component, Custom02Component, Custom03Component
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
