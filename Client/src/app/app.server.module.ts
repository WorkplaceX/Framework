import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { AppModule } from './app.module';

import { AppComponent } from './component';

@NgModule({
    declarations:[],
imports: [
    ServerModule,
    AppModule
],
bootstrap: [AppComponent]
})
export class AppServerModule { }