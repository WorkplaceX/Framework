import { NgModule, enableProdMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
/* GulpFind01 */ import { AppComponent, Selector, Page, Div, LayoutDebug, Button, Literal, Label, Grid, GridRow, GridCell, GridHeader, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective } from './app/component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import * as util from './app/util';
import { HttpModule } from '@angular/http';
import { FormsModule }   from '@angular/forms';

enableProdMode();

@NgModule({
  imports: [NgbModule.forRoot(), BrowserModule, HttpModule, FormsModule],
/* GulpFind02 */ declarations: [AppComponent, Selector, Page, Div, LayoutDebug, Button, Literal, Label, Grid, GridRow, GridCell, GridHeader, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective],
  bootstrap: [AppComponent],
  providers: [
    { provide: 'angularJson', useValue: JSON.stringify({ Name: "app.module.ts=" + util.currentTime() }) },
  ],
  
})
export class AppModule { }
