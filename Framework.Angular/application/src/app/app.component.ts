import { Component } from '@angular/core';
import { DataService } from './data.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'application';

  constructor(public dataService: DataService){
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}
