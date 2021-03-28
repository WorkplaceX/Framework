import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { Page, Selector, Button, Html, Div, DivContainer, BingMap } from './framework/framework.component';
import { BulmaNavbarComponent } from './bulma-navbar/bulma-navbar.component';
import { GridComponent } from './grid/grid.component';
import { BootstrapNavbarComponent } from './bootstrap-navbar/bootstrap-navbar.component';

import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    AppComponent, Selector, Page, Button, Html, Div, DivContainer, BingMap, BulmaNavbarComponent, GridComponent, BootstrapNavbarComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
