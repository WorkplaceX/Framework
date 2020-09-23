import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { Selector, Page, Html, Button, Div, DivContainer, BingMap } from './framework/framework.component';

import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { GridComponent } from './grid/grid.component';
import { BootstrapNavbarComponent } from './bootstrap-navbar/bootstrap-navbar.component';
import { BulmaNavbarComponent } from './bulma-navbar/bulma-navbar.component';
import { Custom01Component } from 'src/Application.Website/Shared/CustomComponent/custom01.component';

@NgModule({
  declarations: [
    AppComponent, Selector, Page, Html, Button, Div, DivContainer, BingMap, GridComponent, BootstrapNavbarComponent, BulmaNavbarComponent, Custom01Component
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
