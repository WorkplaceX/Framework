import { Component } from '@angular/core';
import { DataService, CommandJson } from './data.service';
import { Location, LocationStrategy, PathLocationStrategy, PopStateEvent} from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [Location, {provide: LocationStrategy, useClass: PathLocationStrategy}],
})
export class AppComponent {
  title = 'application';
  localServer = 'localhost:44308/';

  constructor(public dataService: DataService, private location: Location, private http: HttpClient) {
    this.location.onUrlChange((url: string, state: unknown) => {
      var path: string = <string>state; // See also file data.service.ts method call location.go();
      if (path == null) {
        path = "/";
      }
      
      // LinkPost for backward and forward navigation.
      this.dataService.update(<CommandJson> { Command: 17, ComponentId: this.dataService.json.Id, NavigateLinkPath: path});
    });
  }
  
  myClick() {
    this.localServer = "Get...";
    this.http.get("https://localhost:44308/", {responseType: 'text'}).subscribe((data) => this.localServer = data);
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}