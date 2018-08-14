import { Component, ViewEncapsulation, Input } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'data-app', // Make html 5 valid for server side rendering // 'app-root'
  template: `
  <div class="alertError" *ngFor="let item of DataService.alertErrorList$ | async;">{{item}}8</div>

  <h1>Hello World2</h1>
  <button (click)="onClick()">Click</button> <br/>
  Name={{ DataService.json.Name }} <br/>
  Session={{ DataService.json.Session }} <br/>
  SessionApp={{ DataService.json.SessionApp }} <br/>
  SessionState={{ DataService.json.SessionState }} <br/>
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

  i = 0;

  onClick(): void {
    this.DataService.json.Name += ".";
    this.i += 1;
    this.DataService.alertError.next("Error" + this.i);
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
  <div data-Grid *ngIf="json.Type=='Grid'" [json]=json style="display:inline"></div>
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

/* Grid */
@Component({
  selector: '[data-Grid]',
  template: `
  <table [ngClass]="json.CssClass">
    <tr>
      <th *ngFor="let item of json.Header.ColumnList; trackBy trackBy">
        {{ item.Text }}
      </th>
    </tr>
    <tr>
      <th *ngFor="let item of json.Header.ColumnList; trackBy trackBy">
        <input type="text" value="{{ item.SearchText }}">
      </th>
    </tr>
    <tr *ngFor="let row of json.RowList; trackBy trackBy" [ngClass]="{'gridRowIsSelect':row.IsSelect}" (click)="clickRow(row)">
      <td *ngFor="let cell of row.CellList; trackBy trackBy">
        <input type="text" [(ngModel)]="cell.Text" (focusin)=focus(row) (ngModelChange)="ngModelChange(cell)" [ngClass]="{'girdCellIsModify':cell.IsModify}">
        <div *ngIf="cell.IsLookup" class="gridLookup">Lookup</div>
      </td>
    </tr>
  </table>
  `
})
export class Grid {
  constructor(private dataService: DataService){
  }

  @Input() json: any

  ngModelChange(cell) {
    cell.IsModify = true;

    // Merge
    if (cell.MergeId == null) {
      this.dataService.mergeCount += 1;
      cell.MergeId = this.dataService.mergeCount; // Make cell "merge ready".
    }
    if (this.dataService.isRequestPending == true) {
      this.dataService.mergeBufferId = cell.MergeId;
      this.dataService.mergeBufferText = cell.Text; // Buffer user input during pending request.
    }

    this.dataService.update();
  }

  focus(row) {
    if (!row.IsSelect) {
      row.IsClick = true;
      this.dataService.update();
    }
  }

  clickRow(row) {
    if (!row.IsSelect) {
      row.IsClick = true;
      this.dataService.update();
    }
  }

  trackBy(index, item) {
    return index; // or item.id
  }  
}
