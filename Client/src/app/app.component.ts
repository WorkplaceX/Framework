import { Component, ViewEncapsulation, Input } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'data-app', // Make html 5 valid for server side rendering // 'app-root'
  template: `
  <div class="alertError" *ngIf="DataService.alertError != null" >{{ DataService.alertError }}</div>
  <div class="alertInfo" *ngIf="DataService.alertInfo != null" >{{ DataService.alertInfo }}</div>

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
  <div style="display:inline" data-Button *ngIf="json.Type=='Button'" [json]=json></div>
  <div style="display:inline" data-Grid *ngIf="json.Type=='Grid'" [json]=json></div>
  `
})
export class Selector {
  @Input() json: any
}

/* Button */
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
      </td>
    </tr>
  </table>
  `
})
export class Grid {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @Input() json: any
  dataService: DataService;

  ngModelChange(cell) {
    cell.IsModify = true;
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
