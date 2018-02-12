import { Component, Input, ViewChild } from '@angular/core';
import { Directive, ElementRef, Inject, Renderer2 } from '@angular/core';
import { Pipe, PipeTransform } from '@angular/core';
import { DataService } from './dataService';
import  * as util from './util';

/* AppComponent */
@Component({
  selector: '[data-app]', /* Attribute selector "data-App" (lower char because of express engine) */
  template: `
  <div class="selector" data-Selector [json]=item *ngFor="let item of dataService.json.List; trackBy trackBy"></div>  
`,
  providers: [DataService]  
})
export class AppComponent { 
  dataService: DataService;
  jsonText: string;

  constructor(dataService: DataService){
    this.dataService = dataService;
  } 

  trackBy(index: any, item: any) {
    return item.Key;
  }

  clickClient(){
    this.dataService.json.Name += " " + util.currentTime() + ";" 
  } 

  clickServer(){
    this.dataService.update();
  } 

  clickJson() {
    this.jsonText = JSON.stringify(this.dataService.json);
  }
}

/*
  <p>
  json.SelectGridName=({{ dataService.json.GridDataJson?.SelectGridName }})<br />
  json.SelectColumnName=({{ dataService.json.GridDataJson?.SelectColumnName }})<br />
  json.SelectIndex=({{ dataService.json.GridDataJson?.SelectIndex }})<br />
  </p>

  <p>
  json.Name=({{ dataService.json.Name }})<br />
  json.RequestUrl=({{ dataService.json.RequestUrl }})<br />
  json.Session=({{ dataService.json.Session }})<br />
  json.IsBrowser=({{ dataService.json.IsBrowser }})<br />
  RequestCount=({{ dataService.RequestCount }})<br />
  json.ResponseCount=({{ dataService.json.ResponseCount }})<br />
  Version=({{ dataService.json.VersionClient + '; ' + dataService.json.VersionServer }})<br />
  json.ErrorProcess=({{ dataService.json.ErrorProcess }})<br />
  log=({{ dataService.log }})
  </p>
*/

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <div data-Div *ngIf="json.Type=='Div' && !json.IsHide" [json]=json></div>
  <div data-Button *ngIf="json.Type=='Button' && !json.IsHide" [json]=json></div>
  <div data-Literal *ngIf="json.Type=='Literal' && !json.IsHide" [json]=json></div>
  <div data-Label *ngIf="json.Type=='Label' && !json.IsHide" [json]=json></div>
  <div data-Grid *ngIf="json.Type=='Grid' && !json.IsHide" [json]=json></div>
  <div data-GridKeyboard *ngIf="json.Type=='GridKeyboard' && !json.IsHide" [json]=json></div>
  <div data-Page *ngIf="json.Type=='Page' && !json.IsHide" [json]=json></div>
  <div data-GridFieldSingle *ngIf="json.Type=='GridFieldSingle' && !json.IsHide" [json]=json></div>
  <div data-GridFieldWithLabel *ngIf="json.Type=='GridFieldWithLabel' && !json.IsHide" [json]=json></div>
  `
})
export class Selector {
  @Input() json: any
}

/* GridFieldWithLabel */
@Component({
  selector: '[data-GridFieldWithLabel]',
  template: `
  <div [ngClass]="json.CssClass" data-RemoveSelector>
    <div style="overflow: hidden">
      <div class="gridFieldWithLabelLeft">{{json.Text}}</div>
      <div class="gridFieldWithLabelRight">
        <div data-GridField [gridName]=json.GridName [columnName]=json.ColumnName [index]=json.Index></div>
      </div>
    </div>
  </div>  
`
})
export class GridFieldWithLabel {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* GridFieldSingle */
@Component({
  selector: '[data-GridFieldSingle]',
  template: `
  <div [ngClass]="json.CssClass" data-RemoveSelector>
    <div data-GridField></div>
  </div>  
`
})
export class GridFieldSingle {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* Page */
@Component({
  selector: '[data-Page]',
  template: `
  <div [ngClass]="json.CssClass" class="selector" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy data-RemoveSelector"></div>
`
})
export class Page {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* Div */
@Component({
  selector: '[data-Div]',
  template: `
  <div [ngClass]="json.CssClass" data-RemoveSelector>
    <div class="selector" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  </div>  
`
})
export class Div {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* Button */ // See also GridField for button in grid.
@Component({
  selector: '[data-Button]',
  template: `
  <button [ngClass]="json.CssClass" class="btn btn-primary" (click)="click()" data-RemoveSelector>{{ json.Text }}</button>
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

/* Literal */
@Component({
  selector: '[data-Literal]',
  template: `<div [ngClass]="json.CssClass" *ngIf="json.TextHtml" [innerHtml]="json.TextHtml" #div data-RemoveSelector></div>` // See also: https://stackoverflow.com/questions/45459624/angular-4-universal-this-html-charcodeat-is-not-a-function
})
export class Literal {
 
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @ViewChild('div') div:ElementRef;

  ngAfterViewInit() {
    // this.div.nativeElement.innerHTML = this.json.Html; // Not Universal compatible!
  }  

  @Input() json: any
  dataService: DataService;
}

/* Label */
@Component({
  selector: '[data-Label]',
  template: `<div [ngClass]="json.CssClass" data-RemoveSelector>{{ json.Text }}</div>`
})
export class Label {
  @Input() json: any
}

@Pipe({
    name: 'columnIsVisible'
})
export class ColumnIsVisiblePipe implements PipeTransform {
    transform(items: Array<any>): Array<any> {
              if (!items) {
            return items;
        }
        return items.filter(item => item.IsVisible == true);
    }
}

/* Grid */
@Component({
  selector: '[data-Grid]',
  template: `
  <div [ngClass]="json.CssClass" data-RemoveSelector>
    <div style="overflow: hidden">
      <div data-GridColumn [json]=item *ngFor="let item of dataService.json.GridDataJson.ColumnList[json.GridName] | columnIsVisible; trackBy trackBy"></div>
    </div>
    <div data-GridRow [jsonGridDataJson]=dataService.json.GridDataJson [jsonGrid]=json [json]=item *ngFor="let item of dataService.json.GridDataJson.RowList[json.GridName]; trackBy trackBy"></div>
    <button class="btn btn-primary" (click)="clickPageIndex(false)">&nbsp;▲&nbsp;</button> <button class="btn btn-primary" (click)="clickPageIndex(true)">&nbsp;▼&nbsp;</button>
    <button class="btn btn-primary" (click)="clickPageHorizontalIndex(false)">&nbsp;◄&nbsp;</button> <button class="btn btn-primary" (click)="clickPageHorizontalIndex(true)">&nbsp;►&nbsp;</button>
  </div>
  ` // Without style="overflow: hidden", filter column disappears.
})
export class Grid {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  dataService: DataService;
  @Input() json: any

  clickPageIndex(isNext: boolean) {
    if (isNext == true) {
      this.dataService.json.GridDataJson.GridQueryList[this.json.GridName].IsPageIndexNext = true;
    } else {
      this.dataService.json.GridDataJson.GridQueryList[this.json.GridName].IsPageIndexPrevious = true;
    }
    //
    this.dataService.update();
  }

  clickPageHorizontalIndex(isNext: boolean) {
    if (isNext == true) {
      this.dataService.json.GridDataJson.GridQueryList[this.json.GridName].IsPageHorizontalIndexNext = true;
    } else {
      this.dataService.json.GridDataJson.GridQueryList[this.json.GridName].IsPageHorizontalIndexPrevious = true;
    }
    //
    this.dataService.update();
  }

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* GridRow */
@Component({
  selector: '[data-GridRow]',
  template: `
  <div (click)="click()" (mouseover)="mouseOver()" (mouseout)="mouseOut()" [ngClass]="{'select-class1':json.IsSelect==1, 'select-class2':json.IsSelect==2, 'select-class3':json.IsSelect==3, 'gridRowFilter':json.IsFilter}" style="overflow: hidden">
    <div data-GridCell [jsonGrid]=jsonGrid [jsonGridDataJson]=jsonGridDataJson [jsonRow]=json [json]=item *ngFor="let item of jsonGridDataJson.ColumnList[jsonGrid.GridName] | columnIsVisible; trackBy trackBy">
    </div>

    <div *ngIf="json.Error != null" class="ErrorRow">
      {{ json.Error }}
    </div>
  </div>
` // style="overflow: hidden" makes background color visible. See also: https://stackoverflow.com/questions/944663/css-background-color-has-no-effect-on-a-div
})
export class GridRow {
  @Input() json: any;
  @Input() jsonGrid: any;
  @Input() jsonGridDataJson: any;
  dataService: DataService;

  constructor(dataService: DataService){
    this.dataService = dataService;
  }
  
  mouseOver(){
    this.json.IsSelect = this.json.IsSelect | 2;
  }

  mouseOut(){
    this.json.IsSelect = this.json.IsSelect & 1;
  }

  trackBy(index: any, item: any) {
    return item.Key;
  }

  click(){
    this.json.IsClick = true;
    this.dataService.update();
  }
}

/* GridCell */
@Component({
  selector: '[data-GridCell]',
  template: `
  <div (click)="click($event)" class="gridCell" [ngClass]="{'select-class':jsonGridDataJson.CellList[jsonGrid.GridName][json.ColumnName][jsonRow.Index].IsSelect}">
    <div data-GridField [gridName]=jsonGrid.GridName [columnName]=json.ColumnName [index]=jsonRow.Index></div>
  </div>
  `,
  host: {
    '[style.float]' : "'left'",
    '[style.width.%]' : "json.WidthPercent",
    '[style.verticalAlign]' : "'top'" // Prevent when error is shown in cell, text in other cells moves down.
  }
})
//    <div style='margin-right:30px;text-overflow: ellipsis; overflow:hidden;'>
//      {{ jsonGridDataJson.CellList[jsonGrid.GridName][json.ColumnName][jsonRow.Index].T }}&nbsp;
//      <img src='ArrowDown.png' style="width:12px;height:12px;top:8px;position:absolute;right:7px;"/>
//    </div>
export class GridCell {
  @Input() json: any; // Column // Used for ColumnName
  @Input() jsonRow: any; // Used for Index
  @Input() jsonGrid: any; // Used for GridName
  @Input() jsonGridDataJson: any; // Used for Value
  dataService: DataService;

  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  trackBy(index: any, item: any) {
    return item.Key;
  }

  click(event: MouseEvent){
    let gridCell = this.jsonGridDataJson.CellList[this.jsonGrid.GridName][this.json.ColumnName][this.jsonRow.Index];
    if (gridCell.IsClick != true && gridCell.IsSelect != true) { 
      // IsClick might have been set by GridField focus. If IsSelect is already set, do not post.
      gridCell.IsClick = true;
      this.dataService.update();
    }
    event.stopPropagation(); // Prevent underlying GridRow to fire click event.
  }
}

/* GridColumn (Header) */ 
@Component({
  selector: '[data-GridColumn]',
  template: `
  <div (click)="click()" class="gridColumn"><b>{{ json.Text }}</b></div>
  `,
  host: {
    '[style.float]' : "'left'",
    '[style.width.%]' : "json.WidthPercent",
  }
})
export class GridColumn {
  @Input() json: any; // GridColumn
  dataService: DataService;

  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  click(){
    // this.json.IsSelect = !this.json.IsSelect;
    this.json.IsClick = true;
    this.dataService.update();
  }
}

@Directive({
    selector: '[dFocus]'
})
export class FocusDirective {
    @Input()
    focus:boolean;
    constructor(@Inject(ElementRef) private element: ElementRef) {}
    public ngOnChanges(v: any) {
      if (v.focus.currentValue == true){
        if (this.element.nativeElement.focus != null) { // Universal rendering
          this.element.nativeElement.focus();
        }
      }
    }
}

@Directive({
    selector: '[data-RemoveSelector]'
})
export class RemoveSelectorDirective {
    constructor(private el: ElementRef, private renderer: Renderer2) {
    }

    //wait for the component to render completely
    ngOnInit() {
      // console.log(this.el.nativeElement); // this
      // console.log(this.el.nativeElement.parentNode); // span
      // console.log(this.el.nativeElement.parentNode.parentNode); // selector
      //this.el.nativeElement.parentNode.parentNode.parentNode.insertBefore(this.el.nativeElement, this.el.nativeElement.parentNode.parentNode);
      this.renderer.insertBefore(this.el.nativeElement.parentNode.parentNode.parentNode, this.el.nativeElement, this.el.nativeElement.parentNode.parentNode);
      // this.el.nativeElement.childNodes.forEach(element => {
      //   console.log(element.parentNode);
      // });
      // this.el.nativeElement.innerHTML = ".";
      // this.el.nativeElement.parentNode.removeChild(this.el.nativeElement);
      // See also: https://gist.github.com/bhavik07/4a8f2402475c55835679
      // this.renderer.attachViewAfter(this.el.nativeElement.parentNode.parentNode, [this.el.nativeElement]);
    }

    ngOnDestroy() {
      // console.log(this.el.nativeElement.parentNode);
      // this.el.nativeElement.innerHTML = null;
      // this.el.nativeElement.parentNode.removeChild(this.el.nativeElement);
      this.renderer.removeChild(this.el.nativeElement.parentNode, this.el.nativeElement);
    }
}

/* GridField */
@Component({
  selector: '[data-GridField]',
  // See also: http://jsfiddle.net/V79Hn/ for overflow:hidden AND /* GridCell */ [style.verticalAlign]
  template: `
  <div [ngClass]="gridCell().CssClass">
    <div *ngIf="gridCell().CellEnum == null">
      <input type="text" [(ngModel)]="Text" (ngModelChange)="onChange()" placeholder="{{ gridCell().PlaceHolder }}" (focusout)="focus($event, false)" (focusin)="focus($event, true)" />
    </div>

    <button *ngIf="gridCell().CellEnum == 1" class="btn btn-primary" (click)="buttonClick($event)">{{ Text }}</button>
  
    <div *ngIf="gridCell().CellEnum == 2">
      <div [innerHtml]=TextHtml style='overflow:hidden; text-overflow: ellipsis; white-space: nowrap;'></div>
    </div>

    <div *ngIf="gridCell().CellEnum == 3">
      <button class="btn btn-primary" (click)="clickFileUpload()">{{ Text }}</button>
      <input #inputElement type="file" class="btn btn-primary" (change)="changeFileUpload($event)" style='display:none'/>
    </div>

    <div *ngIf="gridCell().E != null" class="ErrorCell">
      {{ gridCell().E }}
    </div>

    <div class="gridLookup" *ngIf="gridCell().IsLookup && gridCell().FocusId == focusId">
      <div>
        <div data-Grid [json]="{ GridName: gridCell().GridNameLookup }"></div>
      </div>
    </div>
  </div>
  `
})
// IsLookup={{ gridCell().IsLookup }}; Focus={{ gridCell().FocusId }}-{{ focusId }}; GridNameLookup={{ gridCell().GridNameLookup }};
// {{focusId + "; " + gridCell().FocusId + "; " + gridCell().FocusIdRequest }}
// <input type="text" class="form-control" [(ngModel)]="Text" (ngModelChange)="onChange()" (dFocus)="focus(true)" (focusout)="focus(false)" [focus]="dataService.json.GridDataJson.FocusGridName==gridName && dataService.json.GridDataJson.FocusColumnName == columnName && dataService.json.GridDataJson.FocusIndex==index" placeholder="{{ gridCell().PlaceHolder }}" />
export class GridField {
  constructor(dataService: DataService){
    this.dataService = dataService;
    //
    this.dataService.idFocusCount += 1;
    this.focusId = this.dataService.idFocusCount;
  }

  dataService: DataService;
  @Input() gridName: any;
  @Input() columnName: any;
  @Input() index: any;
  @ViewChild('inputElement') el:ElementRef;
  focusId: number;

  point() {
    let gridData: any = this.dataService.json.GridDataJson;
    let gridName: string;
    let columnName: string;
    let index: string;
    gridName = this.gridName;
    columnName = this.columnName;
    index = this.index;
    if (gridName == null) {
        gridName = gridData.SelectGridName;
        columnName = gridData.SelectColumnName;
        index = gridData.SelectIndex;
    }
    return { gridData: gridData, gridName: gridName, columnName: columnName, index: index }; // GridName can be null if none is selected.
  }

  focus(event: FocusEvent, value: boolean) {
    if (value == true && this.gridCell().IsClick != true) {
      this.gridCell().FocusIdRequest = this.focusId;
      this.gridCell().IsClick = true;
      this.dataService.update();
    }
    if (value == false) {
      this.gridCell().FocusIdRequest = null; // Prevent multiple FocusIdRequest for slow connection.
      this.gridCell().IsClick = null; // Prevent multiple IsClick for slow connection.
    }
  }

  gridCell() {
    let result : any = null;
    let point = this.point();
    if (point.gridName != null && point.gridData.CellList[point.gridName] != null) { // GridName can be null if none is selected or GridName might not exist.
      result = point.gridData.CellList[point.gridName][point.columnName][point.index];
    }
    if (result == null) {
      result = {};
    }
    return result;
  }

  get Text(): string {
    return this.gridCell().T;
  }

  get TextHtml(): string {
    let result: string = this.Text;
    if (result == null) {
      result = "&nbsp;"; // Avoids cell without height. And Angular4 render error: See also: https://stackoverflow.com/questions/45459624/angular-4-universal-this-html-charcodeat-is-not-a-function
    }
    return result;
  }

  set Text(textNew: string) {
    let gridCell = this.gridCell();
    // Backup old text.
    if (gridCell.IsO == null) { 
      gridCell.IsO = true;
      gridCell.O = gridCell.T;
    }
    // New text back to old text.
    if (gridCell.IsO == true && gridCell.O == textNew) {
      gridCell.IsO = null;
      gridCell.O = null;
    }
    // IsDeleteKey
    let textLength: number = gridCell.T == null ? 0 : gridCell.T.length;
    let textNewLength: number = textNew == null ? 0 : textNew.length;
    let isDeleteKey: boolean = textNewLength < textLength;
    
    gridCell.T = textNew;
    gridCell.IsModify = true;
    gridCell.IsDeleteKey = isDeleteKey;
    // GridSave icon.
    if (gridCell.CssClass == null || gridCell.CssClass.indexOf('gridSave') == -1) {
      gridCell.CssClass = gridCell.CssClass == null ? "" : gridCell.CssClass; // Prevent 'undefined'
      gridCell.CssClass += " gridSave";
    }
  }

  onChange() {
    let isUpdate = false;
    let point = this.point();
    if (point.gridName != null && point.gridData.ColumnList[point.gridName]) { // GridName can be null if none is selected. Or GridName might not exist.
      for (let column of point.gridData.ColumnList[point.gridName]) {
        if (column.ColumnName == point.columnName) {
          isUpdate = column.IsUpdate;
          isUpdate = true; // Always post back.
          break;
        }
      }
    }
    if (isUpdate == true) {
      this.dataService.update();
    }
  }

  buttonClick(event: MouseEvent) {
    this.gridCell().IsModify = true;
    this.onChange();
    event.stopPropagation(); // Prevent underlying GridCell and GridRow to fire click event.
  }

  changeFileUpload(e) {
    var file = e.target.files[0];
    if (!file) {
      return;
    }
    var reader = new FileReader();
    let This: any = this;
    reader.onload = function(e2: any) {
      // console.log(This.el.nativeElement.value);
      var data = e2.target.result;
      // console.log(data);
      This.Text = data;
      This.onChange();
    };
    reader.readAsDataURL(file);
  }

  clickFileUpload() {
    this.el.nativeElement.click();
  }
}

/* GridKeyboard */
@Component({
  selector: '[data-GridKeyboard]',
  template: `
  `,
  host: {
    '(document:keydown)': '_keydown($event)',
  }
})
export class GridKeyboard {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }
  @Input() json: any;
  dataService: DataService;

  next(list: any, current: string, propertyName: string){
    let result : any = {}; // Returns First, Last, Next, Previous
    // First, Last
    for (let key in list){
      if (result.First == null)
        result.First = list[key][propertyName];
      result.Last = list[key][propertyName];
    }
    // Previous, Next
    for (let key in list) {
      if (result.Current != null && result.Next == null) 
        result.Next = list[key][propertyName];
      if (list[key][propertyName] == current) 
        result.Current = list[key][propertyName];
      if (result.Current == null) {
        result.Previous = list[key][propertyName];
      }
    }
    if (result.Current == null) {
      result.Previous = result.First;
      result.Next = result.Last;
    }
    if (result.Previous == null) {
      result.Previous = result.Current;
    }
    if (result.Next == null) {
      result.Next = result.Current;
    }
    return result;
  }

  select() {
    let gridData: any = this.dataService.json.GridDataJson;
    // GridName
    for (let keyGridQuery in gridData.GridQueryList) {
      let gridName = gridData.GridQueryList[keyGridQuery].GridName;
      // ColumnName
      for (let keyColumn in gridData.ColumnList[gridName]) {
        let columnName = gridData.ColumnList[gridName][keyColumn].ColumnName;
        // Index
        for (let keyRow in gridData.RowList[gridName]) {
          let index = gridData.RowList[gridName][keyRow].Index;
          gridData.CellList[gridName][columnName][index].IsSelect = gridData.SelectGridName == gridName && gridData.SelectColumnName == columnName && gridData.SelectIndex == index;
        }
      }
    }
    return;
  }

  public _keydown(event: KeyboardEvent) {
    var gridData: any = this.dataService.json.GridDataJson;
    if (gridData.SelectGridName != null) {
      // Tab
      if (event.keyCode == 9 && event.shiftKey == false) { 
        gridData.SelectColumnName = this.next(gridData.ColumnList[gridData.SelectGridName].filter(item => item.IsVisible == true), gridData.SelectColumnName, "ColumnName").Next;
        this.select();
        event.preventDefault();
      }
      // Tab back
      if (event.keyCode == 9 && event.shiftKey == true) {
        gridData.SelectColumnName = this.next(gridData.ColumnList[gridData.SelectGridName].filter(item => item.IsVisible == true), gridData.SelectColumnName, "ColumnName").Previous;
        this.select();
        event.preventDefault();
      }
      // Up
      if (event.keyCode == 38) {
        gridData.SelectIndex = this.next(gridData.RowList[gridData.SelectGridName], gridData.SelectIndex, "Index").Previous;
        this.select();
        event.preventDefault();
      }
      // Down
      if (event.keyCode == 40) {
        gridData.SelectIndex = this.next(gridData.RowList[gridData.SelectGridName], gridData.SelectIndex, "Index").Next;
        this.select();
        event.preventDefault();
      }
    }
  }
}
