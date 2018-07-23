import { Component } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(public DataService: DataService){

  }

  onClick(): void {
    this.DataService.Json.Name += ".";
  }
}
