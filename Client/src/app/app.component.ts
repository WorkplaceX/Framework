import { Component, ViewEncapsulation, Input } from '@angular/core';
import { DataService } from '../data.service';

@Component({
  selector: 'data-app', // Make html 5 valid for server side rendering // 'app-root'
  template: `
  <!-- /* Debug */
  <div class="alertError" *ngFor="let item of DataService.alertErrorList$ | async;">{{item}}8</div>
  <h1>Hello World</h1>
  <button (click)="onClick()">Click</button> <br/>
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

/* Grid */
@Component({
  selector: '[data-Grid]',
  template: `
  <table [ngClass]="json.CssClass">
    <tr>
      <th *ngFor="let column of json.ColumnList; trackBy trackBy" (click)="clickSort(column, $event);">
        <div style="display:flex; white-space:nowrap;">
          <div style="flex:1; overflow:hidden;">
            <i *ngIf="column.IsSort==false" class="fas fa-caret-up colorWhite"></i>
            <i *ngIf="column.IsSort==true" class="fas fa-caret-down colorWhite"></i>
            {{ column.Text }}
          </div>
          <div style="padding-left:2px;">
            <i *ngIf="column.IsClickSort" class="fas fa-spinner fa-spin colorWhite"></i>
            <i class="fas fa-cog colorWhite colorBlueHover pointer" title="Config data grid column" (click)="clickConfig(column, $event);"></i>
          </div>
        </div>
      </th>
    </tr>
    <tr *ngFor="let row of json.RowList; trackBy trackBy" [ngClass]="{'gridRowIsSelect':row.IsSelect}" (click)="clickRow(row, $event)">
      <td *ngFor="let cell of row.CellList; trackBy trackBy" [ngClass]="{'gridRowFilter':row.RowEnum==1}">
        <div style="display:flex;">

          <!-- /* Red plus sign at begin of field */
          <div style="display:inline-block; padding-right:2px;">
            <svg width="1em" height="1em" version="1.1" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"
              viewBox="0 0 512 512" style="enable-background:new 0 0 512 512;" xml:space="preserve">
              <circle style="fill:#D80027;" cx="256" cy="256" r="256"/>
              <polygon style="fill:#F0F0F0;" points="389.565,211.479 300.522,211.479 300.522,122.435 211.478,122.435 211.478,211.479 
              122.435,211.479 122.435,300.522 211.478,300.522 211.478,389.565 300.522,389.565 300.522,300.522 389.565,300.522 "/>
           </svg>
          </div>
          -->

          <div style="flex:1; overflow:hidden;">
            <input type="text" [(ngModel)]="cell.Text" (focusin)=focus(row) (ngModelChange)="ngModelChange(cell)" [ngClass]="{'girdCellIsModify':cell.IsModify}">
          </div>
          <div style="padding-left:2px">
            <i *ngIf="cell.IsModify" class="fas fa-spinner fa-spin colorBlack"></i>
            <i class="fas fa-arrow-up" style="color:green"></i>
            <!-- /* Lock sign */
            <i class="fas fa-lock colorBlack"></i>
            -->
            <i *ngIf="row.RowEnum==1" class="fas fa-search colorBlack" title="Search"></i>
            <i *ngIf="row.RowEnum==3" class="fas fa-plus" title="Add new data record" style="color:#FACC2E"></i>
          </div>
        </div>
        <div data-Grid *ngIf="cell.IsLookup && json.List?.length > 0" [json]="json.List[0]" class="gridLookup"></div>
      </td>
    </tr>
  </table>

  <div class="colorBlue" [ngClass]="json.CssClass">
    <i class="fas fa-chevron-circle-up colorBlueHover pointer" title="Page up" (click)="clickGrid(1, $event);"></i>
    <i class="fas fa-chevron-circle-down colorBlueHover pointer" title="Page down" (click)="clickGrid(2, $event);"></i>
    &nbsp;&nbsp;
    <i class="fas fa-chevron-circle-left colorBlueHover pointer" title="Navigate left" (click)="clickGrid(3, $event);"></i>
    <i class="fas fa-chevron-circle-right colorBlueHover pointer" title="Navigate right" (click)="clickGrid(4, $event);"></i>
    &nbsp;&nbsp;
    <i class="fas fa-cog colorBlueHover pointer" title="Config data grid" (click)="clickGrid(6, $event);"></i>
    <i class="fas fa-sync colorBlueHover pointer" title="Reload data" (click)="clickGrid(5, $event);"></i>
  </div>
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
    if (!row.IsSelect && !row.IsClick) {
      row.IsClick = true;
      this.dataService.update();
    }
  }

  clickRow(row, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    if (!row.IsSelect && !row.IsClick) {
      row.IsClick = true;
      this.dataService.update();
    }
  }
  
  clickSort(column, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    column.IsClickSort = true;
    this.dataService.update();
  }

  clickConfig(column, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    column.IsClickConfig = true;
    this.dataService.update();
  }

  clickGrid(isClickEnum, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.json.IsClickEnum = isClickEnum;
    this.dataService.update();
  }


  trackBy(index, item) {
    return index; // or item.id
  }  
}
