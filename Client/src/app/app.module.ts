import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { MyComponent } from './Component/MyComponent'
import * as util from './util';
import { HttpModule } from '@angular/http';
import { FormsModule }   from '@angular/forms';
import { enableProdMode } from '@angular/core';

enableProdMode();

import { 
  ColumnIsVisiblePipe,
/* BuildTool */ AppComponent, Selector, GridFieldWithLabel, Page, Div, Button, Literal, Label, Grid, GridRow, GridCell, GridColumn, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective 
} from './component';

@NgModule({
  declarations: [
    MyComponent,
    ColumnIsVisiblePipe,
/* BuildTool */ AppComponent, Selector, GridFieldWithLabel, Page, Div, Button, Literal, Label, Grid, GridRow, GridCell, GridColumn, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective
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
