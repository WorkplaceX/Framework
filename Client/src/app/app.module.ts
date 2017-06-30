import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MyComponent } from './Component/MyComponent'
import * as util from './util';
import { HttpModule } from '@angular/http';
import { FormsModule }   from '@angular/forms';

import { 
/* BuildTool */ AppComponent, Selector, Page, Div, LayoutDebug, Button, Literal, Label, Grid, GridRow, GridCell, GridHeader, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective 
} from './component';

@NgModule({
  declarations: [
    MyComponent,
/* BuildTool */ AppComponent, Selector, Page, Div, LayoutDebug, Button, Literal, Label, Grid, GridRow, GridCell, GridHeader, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'client'}),
    HttpModule, 
    FormsModule
  ],
  providers: [
    { 
      provide: 'angularJson', useValue: JSON.stringify(
        {
          Name: "app.module.ts=" + util.currentTime()
        }) 
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
