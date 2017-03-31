import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UniversalModule, isBrowser, isNode } from 'angular2-universal/node'; // for AoT we need to manually split universal packages

import { AppModule, AppComponent } from './+app/app.module';
import { SharedModule } from './+app/shared/shared.module';
import { CacheService } from './+app/shared/cache.service';

// Will be merged into @angular/platform-browser in a later release
// see https://github.com/angular/angular/pull/12322
import { Meta } from './angular2-meta';

export function getLRU() {
  return new Map();
}
export function getRequest() {
  return Zone.current.get('req') || {};
}
export function getResponse() {
  return Zone.current.get('res') || {};
}

// TODO(gdi2290): refactor into Universal
export const UNIVERSAL_KEY = 'UNIVERSAL_CACHE';

export function currentTime(){
    var result = new Date();
    //
    var hour = result.getHours();
    var hourString = hour.toString();
    if (hour < 10) {
        hourString = '0' + hourString;
    }
    //
    var minute = result.getMinutes()
    var minuteString = minute.toString();
    if (minute < 10) {
        minuteString = "0" + minuteString;
    }
    //
    var second = result.getSeconds()
    var secondString = second.toString();
    if (second < 10) {
        secondString = "0" + secondString;
    }
    //
    return hourString + ":" + minuteString + "." + secondString;
}

function showObject(obj) {
  var result = "";
  for (var p in obj) {
    if( obj.hasOwnProperty(p) ) {
      // result += p + " , " + obj[p] + "\n";
      var value = 'null';
      if (obj[p] != null){
        try
        {
          value = String(obj[p]);
        }
        catch(err)
        {
          value = '?';
        }
      }
      result += p + "=" + value + "; ";
    } 
  }              
  return result;
}

export function data(){
  var result = getRequest().body;
  if (result.Name != undefined){
    return JSON.stringify(result);
  }
  var data = getRequest().url; // showObject(getRequest());
  return JSON.stringify({ Name: "node.module.ts=" + currentTime() + data });
}

@NgModule({
  bootstrap: [ AppComponent ],
  imports: [
    // MaterialModule.forRoot() should be included first
    UniversalModule, // BrowserModule, HttpModule, and JsonpModule are included

    FormsModule,

    SharedModule.forRoot(),
    AppModule,
  ],
  providers: [
    { provide: 'isBrowser', useValue: isBrowser },
    { provide: 'isNode', useValue: isNode },

    { provide: 'req', useFactory: getRequest },
    { provide: 'res', useFactory: getResponse },

    { provide: 'LRU', useFactory: getLRU, deps: [] },

    CacheService,

    Meta,

    { provide: 'angularJson', useFactory: data },
    
  ]
})
export class MainModule {
  constructor(public cache: CacheService) {

  }

  /**
   * We need to use the arrow function here to bind the context as this is a gotcha
   * in Universal for now until it's fixed
   */
  universalDoDehydrate = (universalCache) => {
    universalCache[CacheService.KEY] = JSON.stringify(this.cache.dehydrate());
  }

 /**
  * Clear the cache after it's rendered
  */
  universalAfterDehydrate = () => {
    // comment out if LRU provided at platform level to be shared between each user
    this.cache.clear();
  }
}
