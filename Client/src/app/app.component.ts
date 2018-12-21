import { Component, ViewEncapsulation, Input } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'data-app', // Make html 5 valid for server side rendering // 'app-root'
  template: `
  <!-- /* Debug */
  <div class="alertError" *ngFor="let item of DataService.alertErrorList$ | async;">{{item}}8</div>
  <h1>Hello World</h1>
  Name={{ DataService.json.Name }} <br/>
  Session={{ DataService.json.Session }} <br/>
  SessionApp={{ DataService.json.SessionApp }} <br/>
  SessionState={{ DataService.json.SessionState }} <br/>
  <input type="text" placeholder="Enter email"><br/>
  IsServerSideRendering={{ DataService.json.IsServerSideRendering }} <br/>
  Version={{ DataService.json.Version }} {{ DataService.json.VersionBuild }} <br/>
  VersionBuild.Client = {{ DataService.VersionBuild }}
  -->
  <div style="display:inline" class="selector" data-Selector [json]=item *ngFor="let item of DataService.json.List; trackBy trackBy"></div>  
  `,
  styleUrls: ['./app.component.scss'],
  encapsulation: ViewEncapsulation.None // Prevent html 5 invalid attributes like "_nghost-sc0", "_ngcontent-sc0"
})
export class AppComponent {
  constructor(public DataService: DataService){
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <div data-Button *ngIf="json.Type=='Button'" [json]=json style="display:inline"></div>
  <div data-Div *ngIf="json.Type=='Div'" [json]=json [ngClass]="json.CssClass"></div>
  <div data-BootstrapNavbar *ngIf="json.Type=='BootstrapNavbar'" [json]=json style="display:inline"></div>
  <div data-Grid *ngIf="json.Type=='Grid' && !json.IsHide" [json]=json style="display:inline"></div>
  <div data-Page *ngIf="json.Type=='Page' && !json.IsHide" [json]=json style="display:inline"></div>
  <div data-Html *ngIf="json.Type=='Html'" [json]=json style="display:inline"></div>
  `
})
export class Selector {
  @Input() json: any
}

/* Page */
@Component({
  selector: '[data-Page]',
  template: `
  <div style="display:inline" class="selector" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>  
  `
})
export class Page {
  @Input() json: any
  dataService: DataService;

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* Html */
@Component({
  selector: '[data-Html]',
  template: `<div [ngClass]="json.CssClass" [innerHtml]="json.TextHtml" style="display:inline"></div>`
})
export class Html {
  @Input() json: any
}

/* Button */
@Component({
  selector: '[data-Button]',
  template: `
  <button [ngClass]="json.CssClass" (click)="click();">{{ json.Text }}</button>
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

/* Div */
@Component({
  selector: '[data-Div]',
  template: `
  <div style="display:inline" class="selector" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>  
  `
})
export class Div {
  @Input() json: any
  dataService: DataService;

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

