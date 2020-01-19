import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppComponent, Selector, Page, Html, Button, Div, DivContainer } from './app.component';
import { Grid } from './grid/grid.component';
import { Grid2 } from './grid/grid2.component';
import { BootstrapNavbar } from './bootstrapNavbar/bootstrapNavbar.component';

@NgModule({
  declarations: [
    AppComponent, Selector, Page, Html, Button, Div, DivContainer, Grid, Grid2, BootstrapNavbar
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'serverApp' }),
    HttpClientModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
