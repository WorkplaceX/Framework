import { Component } from '@angular/core';
import { CommandJson, DataService } from './data.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-root',
  template: `<div style="display:inline" data-Selector [json]=item *ngFor="let item of dataService.json.List; trackBy trackBy"></div>`,
})
export class AppComponent {
  title = 'application';

  constructor(public dataService: DataService, private location: Location) { // import { Location } from '@angular/common';
    this.location.onUrlChange((url: string, state: unknown) => {
      if (state != undefined) { // undefined if navigation happens on same page. For example user clicked named anchor link.
        var path: string = <string>state; // See also file data.service.ts method call location.go();
        if (path == null) {
          path = "/";
        }

        // Make sure event origin comes from user and not data.service.ts NavigatePathAddHistory.
        if (dataService.json.NavigatePathAddHistory == undefined) {
          // User clicked backward or forward button in browser.
          this.dataService.update(<CommandJson> { CommandEnum: 17, ComponentId: this.dataService.json.Id, NavigatePath: path});
        }
        dataService.json.NavigatePathAddHistory = undefined;
      }
    });
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}
