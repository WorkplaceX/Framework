import { Component } from '@angular/core';
import { DataService, CommandJson } from './data.service';
import { Location, LocationStrategy, PathLocationStrategy, PopStateEvent} from '@angular/common';

@Component({
  selector: 'app-root',
  template: `<div style="display:inline" data-Selector [json]=item *ngFor="let item of dataService.json.List; trackBy trackBy"></div>`,
  providers: [Location, {provide: LocationStrategy, useClass: PathLocationStrategy}],
})
export class AppComponent {
  title = 'application';

  constructor(public dataService: DataService, private location: Location) {
    this.location.onUrlChange((url: string, state: unknown) => {
      var path: string = <string>state; // See also file data.service.ts method call location.go();
      if (path == null) {
        path = "/";
      }
      
      // LinkPost for backward and forward navigation.
      this.dataService.update(<CommandJson> { Command: 17, ComponentId: this.dataService.json.Id, NavigateLinkPath: path});
    });
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}