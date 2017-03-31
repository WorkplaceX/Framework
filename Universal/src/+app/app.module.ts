import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { HomeModule } from './+home/home.module';
import { AboutModule } from './+about/about.module';
import { TodoModule } from './+todo/todo.module';

import { SharedModule } from './shared/shared.module';

/* GulpFind01 */ import { AppComponent, Selector, LayoutContainer, LayoutRow, LayoutCell, LayoutDebug, Button, InputX, Label, Grid, GridRow, GridCell, GridHeader, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective } from './app.component';


@NgModule({
/* GulpFind02 */ declarations: [AppComponent, Selector, LayoutContainer, LayoutRow, LayoutCell, LayoutDebug, Button, InputX, Label, Grid, GridRow, GridCell, GridHeader, GridField, GridKeyboard, FocusDirective, RemoveSelectorDirective],
  imports: [
    SharedModule
  ]
})
export class AppModule {
}

export { AppComponent } from './app.component';
