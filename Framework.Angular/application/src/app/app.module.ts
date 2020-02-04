import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent, Selector, Page, Html, Button, Div, DivContainer, BingMap} from './app.component';
import { Grid } from './grid/grid.component';
import { BootstrapNavbar } from './bootstrapNavbar/bootstrapNavbar.component';

@NgModule({
  declarations: [
    AppComponent, Selector, Page, Html, Button, Div, DivContainer, Grid, BootstrapNavbar, BingMap
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
