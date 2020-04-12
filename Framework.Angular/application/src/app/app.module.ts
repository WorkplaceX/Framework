import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { FrameworkComponent, Selector, Page, Html, Button, Div, DivContainer, BingMap } from './framework/framework.component';

import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { GridComponent } from './grid/grid.component';
import { BootstrapNavbarComponent } from './bootstrap-navbar/bootstrap-navbar.component';
import { Custom01Component } from 'src/Application.Website/Shared/CustomComponent/custom01.component';

@NgModule({
  declarations: [
    AppComponent, FrameworkComponent, Selector, Page, Html, Button, Div, DivContainer, BingMap, GridComponent, BootstrapNavbarComponent, Custom01Component
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
