import { Component, ViewEncapsulation, Input } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'data-app', // Make html 5 valid for server side rendering // 'app-root'
  template: `
  <h1>Hello World2</h1>
  <button (click)="onClick()">Click</button> <br/>
  Name={{ DataService.json.Name }} <br/>
  <input type="text" placeholder="Enter email"><br/>
  IsServerSideRendering={{ DataService.json.IsServerSideRendering }} <br/>
  Version={{ DataService.json.Version }} {{ DataService.json.VersionBuild }} <br/>
  VersionBuild.Client = {{ DataService.VersionBuild }}
  <div style="display:inline" class="selector" data-Selector [json]=item *ngFor="let item of DataService.json.List; trackBy trackBy"></div>  
  `,
  styleUrls: ['./app.component.scss'],
  encapsulation: ViewEncapsulation.None // Prevent html 5 invalid attributes like "_nghost-sc0", "_ngcontent-sc0"
})
export class AppComponent {
  constructor(public DataService: DataService){

  }

  onClick(): void {
    this.DataService.json.Name += ".";
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <div style="display:inline" data-Button *ngIf="json.Type=='Button' && !json.IsHide" [json]=json></div>
  `
})
export class Selector {
  @Input() json: any
}

/* Button */ // See also GridField for button in grid.
@Component({
  selector: '[data-Button]',
  template: `
  <button [ngClass]="json.CssClass" (click)="click()">{{ json.Text }}</button>
  `
})
export class Button {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @Input() json: any
  dataService: DataService;

  click(){
    this.json.IsClick = true;
    this.dataService.update();
  } 
}
