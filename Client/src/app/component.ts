import { Component, Input, ViewChild } from '@angular/core';
import { Directive, ElementRef, Inject, Renderer2 } from '@angular/core';
import { DataService } from './dataService';
import  * as util from './util';

/* AppComponent */
@Component({
  selector: '[app]',
  template: `
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
  <Selector [json]=item *ngFor="let item of dataService.json.List; trackBy trackBy"></Selector>  
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

/* Selector */
@Component({
  selector: 'Selector',
  template: `
  <span div *ngIf="json.Type=='Div' && !json.IsHide" [json]=json></span>
  <span button *ngIf="json.Type=='Button' && !json.IsHide" [json]=json></span>
  <Literal *ngIf="json.Type=='Literal' && !json.IsHide" [json]=json></Literal>
  <Label *ngIf="json.Type=='Label' && !json.IsHide" [json]=json></Label>
  <span grid *ngIf="json.Type=='Grid' && !json.IsHide" [json]=json></span>
  <GridKeyboard *ngIf="json.Type=='GridKeyboard' && !json.IsHide" [json]=json></GridKeyboard>
  <div GridField *ngIf="json.Type=='GridField' && !json.IsHide" [json]=json></div>
  <Page *ngIf="json.Type=='Page' && !json.IsHide" [json]=json></Page>
  <!-- <LayoutDebug [json]=json></LayoutDebug> -->
`
})
export class Selector {
  @Input() json: any
}

/* Page */
@Component({
  selector: 'Page',
  template: `
  Text={{ json.Text }}
  <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
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
  selector: '[div]',
  template: `
  <div [ngClass]="json.CssClass" removeSelector>
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class Div {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* LayoutDebug */
@Component({
  selector: 'LayoutDebug',
  template: `
  <div style='border:1px solid; padding:2px; margin:2px; background-color:yellow;'>
    Text={{ json.Text }}
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutDebug {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* Button */
@Component({
  selector: '[button]',
  template: `
  <button type="text" [ngClass]="json.CssClass" class="btn btn-primary" (click)="click()" removeSelector>{{ json.Text }}</button>
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
  selector: 'Literal',
  template: `<div #div [ngClass]="json.CssClass"></div>`
})
export class Literal {
 
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @ViewChild('div') div:ElementRef;

  ngAfterViewInit() {
    this.div.nativeElement.innerHTML = this.json.Html;
  }  

  @Input() json: any
  dataService: DataService;
}

/* Label */
@Component({
  selector: 'Label',
  template: `{{ json.Text }}`
})
export class Label {
  @Input() json: any
}

/* Grid */
@Component({
  selector: '[grid]',
  template: `
  <div removeSelector>
    <div [ngClass]="json.CssClass" style="white-space: nowrap;">
      <div gridHeader style="display:inline-block; overflow: hidden;" [json]=item *ngFor="let item of dataService.json.GridDataJson.ColumnList[json.GridName]; trackBy trackBy"></div>
    </div>
    <div gridRow [jsonGridDataJson]=dataService.json.GridDataJson [jsonGrid]=json [json]=item *ngFor="let item of dataService.json.GridDataJson.RowList[json.GridName]; trackBy trackBy"></div>
  </div>
  `
})
export class Grid {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  dataService: DataService;
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* GridRow */
@Component({
  selector: '[gridRow]',
  template: `
  <div (click)="click()" (mouseover)="mouseOver()" (mouseout)="mouseOut()" [ngClass]="{'select-class1':json.IsSelect==1, 'select-class2':json.IsSelect==2, 'select-class3':json.IsSelect==3}" style="white-space: nowrap;">
    <div class="GridCell" [jsonGrid]=jsonGrid [jsonGridDataJson]=jsonGridDataJson [jsonRow]=json [json]=item *ngFor="let item of jsonGridDataJson.ColumnList[jsonGrid.GridName]; trackBy trackBy"></div>
    <div *ngIf="json.Error != null" style="white-space: normal;" class="ErrorRow">
      {{ json.Error }}
    </div>
  </div>
  `,
  styles: [`
  .select-class1 {
    background-color: rgba(255, 255, 0, 0.5);
  }
  .select-class2 {
    background-color: rgba(255, 255, 0, 0.2);
  }
  .select-class3 {
    background-color: rgba(255, 255, 0, 0.7);
  }
  `]
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
  selector: '.GridCell',
  template: `
  <div (click)="click($event)" [ngClass]="{'select-class':jsonGridDataJson.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].IsSelect}">
    <div GridField [gridName]=jsonGrid.GridName [fieldName]=json.FieldName [index]=jsonRow.Index></div>
    <div *ngIf="jsonGridDataJson.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].E != null" class="ErrorCell" style="white-space: normal;">
      {{ jsonGridDataJson.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].E }}
    </div>
  </div>
  `,
  styles: [`
  .select-class {
    border:solid 2px blue;
  }
  `],
  host: {
    '[style.display]' : "'inline-block'",
    '[style.position]' : "'relative'",
    '[style.width.%]' : "json.WidthPercent",
    '[style.verticalAlign]' : "'top'"
  }
})
//    <div style='margin-right:30px;text-overflow: ellipsis; overflow:hidden;'>
//      {{ jsonGridDataJson.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].T }}&nbsp;
//      <img src='ArrowDown.png' style="width:12px;height:12px;top:8px;position:absolute;right:7px;"/>
//    </div>
export class GridCell {
  @Input() json: any; // Column // Used for FieldName
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
    this.jsonGridDataJson.CellList[this.jsonGrid.GridName][this.json.FieldName][this.jsonRow.Index].IsClick = true;
    this.jsonRow.IsClick = true;
    this.dataService.update();
    event.stopPropagation();
  }
}

/* GridHeader */
@Component({
  selector: '[gridHeader]',
  template: `
  <div (click)="click()" [ngClass]="{'select-class':json.IsSelect}" style="display:inline-block;"><b>{{ json.Text }}</b></div>
  `,
  styles: [`
  .select-class {
    background-color: rgba(255, 255, 0, 0.7);
  }
  `],
  host: {
    '[style.width.%]' : "json.WidthPercent",
  }
})
export class GridHeader {
  @Input() json: any; // GridColumn
  dataService: DataService;

  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  click(){
    this.json.IsSelect = !this.json.IsSelect;
    this.json.IsClick = true;
    this.dataService.update();
  }
}

@Directive({
    selector: '[focus]'
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
    selector: '[removeSelector]'
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
  selector: '[GridField]',
  // See also: http://jsfiddle.net/V79Hn/ for overflow:hidden AND /* GridCell */ [style.verticalAlign]
  template: `
  <div>
    <div *ngIf="gridCell().CellEnum==null" style='margin-right:18px'>
      <input type="text" class="form-control" [(ngModel)]="Text" (ngModelChange)="onChange()" (focus)="focus(true)" (focusout)="focus(false)" [focus]="dataService.json.GridDataJson.FocusIndex==index && dataService.json.GridDataJson.FocusFieldName == fieldName" placeholder="Empty" />
    </div>
    <img *ngIf="gridCell().CellEnum==null" src='ArrowDown.png' style="width:12px;height:12px;top:4px;position:absolute;right:4px;"/>

    <div *ngIf="gridCell().CellEnum==2" style='display: inline-block; width:100%;'>
      <div [innerHtml]=Text style='overflow:hidden; text-overflow: ellipsis;'></div>
    </div>

    <button *ngIf="gridCell().CellEnum==1" type="text" class="btn btn-primary" (click)="buttonClick()">{{ Text }}</button>

    <div *ngIf="gridCell().CellEnum==3">
      <button class="btn btn-primary" (click)="clickFileUpload()">{{ Text }}</button>
      <input #inputElement type="file" class="btn btn-primary" (change)="changeFileUpload($event)" style='display:none'/>
    </div>

  </div>
  `
})
export class GridField {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  dataService: DataService;
  @Input() gridName: any;
  @Input() fieldName: any;
  @Input() index: any;
  @Input() json: any;
  @ViewChild('inputElement') el:ElementRef;

  point() {
    let gridData: any = this.dataService.json.GridDataJson;
    let gridName: string = gridData.FocusGridName;
    let fieldName: string = gridData.FocusFieldName;
    let index: string = gridData.FocusIndex;
    if (this.json != null) {
      if (this.json.GridName != null) {
        gridName = this.json.GridName;
      }
      if (this.json.FieldName != null) {
        fieldName = this.json.FieldName;
      }
      if (this.json.Index != null) {
        fieldName = this.json.Index;
      }
    } else {
      if (this.gridName != null){
        gridName = this.gridName;
      }
      if (this.fieldName != null){
        fieldName = this.fieldName;
      }
      if (this.index != null){
        index = this.index;
      }
    }
    return { gridData: gridData, gridName: gridName, fieldName: fieldName, index: index }; // GridName can be null if no focus is set.
  }

  gridCell() {
    let result : any = null;
    let point = this.point();
    if (point.gridName != null) { 
      result = point.gridData.CellList[point.gridName][point.fieldName][point.index]; // GridName can be null if no focus is set.
    }
    if (result == null) {
      result = {};
    }
    return result;
  }

  get Text(): string {
    return this.gridCell().T;
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
    gridCell.T = textNew;
    gridCell.IsModify = true;
  }

  onChange() {
    let isUpdate = false;
    let point = this.point();
    if (point.gridName != null) { 
      // GridName can be null if no focus is set.
      for (let column of point.gridData.ColumnList[point.gridName]) {
        if (column.FieldName == point.fieldName) {
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

  focus(isFocus: boolean) {
    if (this.json !=  null) {
      // this.json can be null
      this.json.IsFocus = isFocus;
    }
  }

  buttonClick() {
    this.gridCell().IsModify = true;
    this.onChange();
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
      console.log(data);
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
  selector: 'GridKeyboard',
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
      // FieldName
      for (let keyColumn in gridData.ColumnList[gridName]) {
        let fieldName = gridData.ColumnList[gridName][keyColumn].FieldName;
        // Index
        for (let keyRow in gridData.RowList[gridName]) {
          let index = gridData.RowList[gridName][keyRow].Index;
          gridData.CellList[gridName][fieldName][index].IsSelect = gridData.FocusGridName == gridName && gridData.FocusFieldName == fieldName && gridData.FocusIndex == index;
        }
      }
    }
    return;
  }

  public _keydown(event: KeyboardEvent) {
    var gridData: any = this.dataService.json.GridDataJson;
    if (gridData.FocusGridName != null) {
      // Tab
      if (event.keyCode == 9 && event.shiftKey == false) { 
        gridData.FocusFieldName = this.next(gridData.ColumnList[gridData.FocusGridName], gridData.FocusFieldName, "FieldName").Next;
        this.select();
        event.preventDefault();
      }
      // Tab back
      if (event.keyCode == 9 && event.shiftKey == true) {
        gridData.FocusFieldName = this.next(gridData.ColumnList[gridData.FocusGridName], gridData.FocusFieldName, "FieldName").Previous;
        this.select();
        event.preventDefault();
      }
      // Up
      if (event.keyCode == 38) {
        gridData.FocusIndex = this.next(gridData.RowList[gridData.FocusGridName], gridData.FocusIndex, "Index").Previous;
        this.select();
        event.preventDefault();
      }
      // Down
      if (event.keyCode == 40) {
        gridData.FocusIndex = this.next(gridData.RowList[gridData.FocusGridName], gridData.FocusIndex, "Index").Next;
        this.select();
        event.preventDefault();
      }
    }
  }
}
