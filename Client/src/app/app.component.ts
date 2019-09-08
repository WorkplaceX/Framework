import { Component, ViewEncapsulation, Input, Renderer2 } from '@angular/core';
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
  <div style="display:inline" data-Selector [json]=item *ngFor="let item of DataService.json.List; trackBy trackBy"></div>  
  `,
  styleUrls: ['./app.component.scss'],
  encapsulation: ViewEncapsulation.None // Prevent html 5 invalid attributes like "_nghost-sc0", "_ngcontent-sc0"
})
export class AppComponent {
  constructor(public DataService: DataService, private renderer: Renderer2 ){
    if (this.DataService.json.IsBootstrapModal == true)
    {
      // TODO detect changes!
      // this.renderer.addClass(document.body, 'modal-open');
      // this.renderer.removeClass(document.body, 'modal-open');
    }
  }

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <div data-Button style="display:inline" *ngIf="json.Type=='Button'" [json]=json></div>
  <div data-Div [ngClass]="json.CssClass" *ngIf="json.Type=='Div'" [json]=json></div>
  <div data-DivContainer [ngClass]="json.CssClass" *ngIf="json.Type=='DivContainer'" [json]=json></div>
  <div data-BootstrapNavbar [ngClass]="json.CssClass" *ngIf="json.Type=='BootstrapNavbar'" [json]=json></div>
  <div data-Grid [ngClass]="json.CssClass" *ngIf="json.Type=='Grid' && !json.IsHide" [json]=json></div>
  <div data-Page [ngClass]="json.CssClass" *ngIf="json.Type=='Page' && !json.IsHide" [json]=json></div>
  <div data-Html style="display:inline" *ngIf="json.Type=='Html'" [json]=json></div>
  `
})
export class Selector {
  @Input() json: any
}

/* Page */
@Component({
  selector: '[data-Page]',
  template: `
  <div style="display:inline" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
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
  template: `<div style="display:inline" [ngClass]="json.CssClass" [innerHtml]="json.TextHtml"></div>`
})
export class Html {
  @Input() json: any
}

/* Button */
@Component({
  selector: '[data-Button]',
  template: `
  <button [ngClass]="json.CssClass" (click)="click();" [innerHtml]="json.TextHtml"></button>
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
  <div style="display:inline" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class Div {
  @Input() json: any
  dataService: DataService;

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* DivContainer */
@Component({
  selector: '[data-DivContainer]',
  template: `
  <div [ngClass]="item.CssClass" data-Div [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class DivContainer {
  @Input() json: any
  
  trackBy(index, item) {
    return index; // or item.id
  }
}
  