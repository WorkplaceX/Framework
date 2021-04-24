import { Component } from '@angular/core';
import { CommandJson, DataService } from './data.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-root',
  template: `<div style="display:inline" data-Selector [json]=item *ngFor="let item of dataService.json.List; trackBy trackBy"></div>`,
})
export class AppComponent {
  title = 'application';

  private path: string = ""; // Is for example "/about". 

  constructor(public dataService: DataService, private location: Location) { // import { Location } from '@angular/common';
    this.location.onUrlChange((url: string, state: unknown) => {
      if (this.path != this.location.path(false)) { // Ignor in page named anchor navigation
        this.path = this.location.path(false); // Path without named anchor in url and no trailing slash.
        let path: string = <string>state; // This is identical to method location.GetState(); See also file data.service.ts method location.go(); where this state is set. Path is for example "/about/";
        if (dataService.json.NavigatePathAddHistory == undefined) { // Make sure event origin comes from user and not from data.service.ts NavigatePathAddHistory.
          if (path == null) { // Nothing in state for example because of in page named anchor navigation.
            path = window.location.pathname; // For example "/about/"
          }
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
