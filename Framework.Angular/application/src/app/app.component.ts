import { Component } from '@angular/core';
import { DataService, CommandJson } from './data.service';
import {Location, LocationStrategy, PathLocationStrategy, PopStateEvent} from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
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
      this.dataService.update(<CommandJson> { Command: 16, ComponentId: this.dataService.json.Id, LinkPostPath: path, LinkPostPathIsAddHistory: false}); 
    });
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}
