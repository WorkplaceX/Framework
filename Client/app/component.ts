import { Component, Input } from '@angular/core';
import { Directive, ElementRef, Inject, Renderer } from '@angular/core';
import { DataService } from './dataService';
import  * as util from './util';

/* AppComponent */
@Component({
  selector: 'app',
  template: `
  <p>
  json.Name=({{ dataService.json.Name }})<br />
  json.Session=({{ dataService.json.Session }})<br />
  json.IsBrowser=({{ dataService.json.IsBrowser }})<br />
  RequestCount=({{ dataService.RequestCount }})<br />
  json.ResponseCount=({{ dataService.json.ResponseCount }})<br />
  Version=({{ dataService.json.VersionClient + '; ' + dataService.json.VersionServer }})<br />
  json.ErrorProcess=({{ dataService.json.ErrorProcess }})<br />
  log=({{ dataService.log }})
  </p>
  <Selector [json]=item *ngFor="let item of dataService.json.List; trackBy:fn"></Selector>
`,
  providers: [DataService]  
})
export class AppComponent { 
  dataService: DataService;
  jsonText: string;

  constructor(dataService: DataService){
    this.dataService = dataService;
  } 

  fn() {
    return "0";
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
  <LayoutContainer *ngIf="json.Type=='LayoutContainer'" [json]=json></LayoutContainer>
  <LayoutRow *ngIf="json.Type=='LayoutRow'" [json]=json></LayoutRow>
  <LayoutCell *ngIf="json.Type=='LayoutCell'" [json]=json></LayoutCell>
  <ButtonX *ngIf="json.Type=='Button'" [json]=json></ButtonX>
  <Literal *ngIf="json.Type=='Literal'" [json]=json></Literal>
  <Label *ngIf="json.Type=='Label'" [json]=json></Label>
  <Grid *ngIf="json.Type=='Grid'" [json]=json></Grid>
  <GridKeyboard *ngIf="json.Type=='GridKeyboard'" [json]=json></GridKeyboard>
  <GridField *ngIf="json.Type=='GridField'" [json]=json></GridField>
  <!-- <LayoutDebug [json]=json></LayoutDebug> -->
`
})
export class Selector {
  @Input() json: any
}

/* LayoutContainer */
@Component({
  selector: 'LayoutContainer',
  template: `
  <div [ngClass]="json.Class" class='container' removeSelector>
    Text={{ json.Text }}
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutContainer {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* LayoutRow */
@Component({
  selector: 'LayoutRow',
  template: `
  <div [ngClass]="json.Class" class='row' removeSelector>
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutRow {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Key;
  }
}

/* LayoutCell */
@Component({
  selector: 'LayoutCell',
  template: `
  <div [ngClass]="json.Class" [class.col-sm-6]='true' removeSelector>
    Text={{ json.Text }}
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutCell {
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
  selector: 'ButtonX',
  template: `<button type="text" [ngClass]="json.Class" class="btn btn-primary" (click)="click()">{{ json.Text }}</button>`
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
  template: `<div [ngClass]="json.Class" [innerHTML]=json.Html></div>`
})
export class Literal {
  constructor(dataService: DataService){
    this.dataService = dataService;
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
  selector: 'Grid',
  template: `
  <div [ngClass]="json.Class" style="white-space: nowrap;">
  <GridHeader [json]=item *ngFor="let item of dataService.json.GridDataJson.ColumnList[json.GridName]; trackBy trackBy"></GridHeader>
  </div>
  <GridRow [jsonGridDataJson]=dataService.json.GridDataJson [jsonGrid]=json [json]=item *ngFor="let item of dataService.json.GridDataJson.RowList[json.GridName]; trackBy trackBy"></GridRow>
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
  selector: 'GridRow',
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
  <div (click)="click($event)" [ngClass]="{'select-class':jsonGridDataJson.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].IsSelect}" >
    <div style='margin-right:30px;text-overflow: ellipsis; overflow:hidden;'>
      {{ jsonGridDataJson.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].T }}&nbsp;
      <img src='ArrowDown.png' style="width:12px;height:12px;top:8px;position:absolute;right:7px;"/>
    </div>
    <GridField [gridName]=jsonGrid.GridName [fieldName]=json.FieldName [index]=jsonRow.Index></GridField>
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
    '[style.width.%]' : "json.WidthPercent"
  }
})
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
  selector: 'GridHeader',
  template: `
  <div (click)="click()" [ngClass]="{'select-class':json.IsSelect}" style="display:inline-block; overflow: hidden;" [style.width.%]=json.WidthPercent><b>{{ json.Text }}</b></div>
  `,
  styles: [`
  .select-class {
    background-color: rgba(255, 255, 0, 0.7);
  }
  `]
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
    constructor(private el: ElementRef, private renderer: Renderer) {
    }

    //wait for the component to render completely
    ngOnInit() {
      this.renderer.attachViewAfter(this.el.nativeElement.parentNode.parentNode, [this.el.nativeElement]);
    }
}

/* GridField */
@Component({
  selector: 'GridField',
  template: `
  <input type="text" class="form-control" [(ngModel)]="Text" (ngModelChange)="onChange()" (focus)="focus(true)" (focusout)="focus(false)" [focus]="dataService.json.GridDataJson.FocusIndex==index && dataService.json.GridDataJson.FocusFieldName == fieldName" placeholder="Empty" />
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
    gridCell.E = null; // Clear cell error
    this.rowErrorClear(); // Clear row error
  }

  rowErrorClear() {
    let gridData: any = this.dataService.json.GridDataJson;
    if (this.gridName in gridData.RowList) {
      for (let keyRow in gridData.RowList[this.gridName]) {
        let index = gridData.RowList[this.gridName][keyRow].Index;
        if (index == this.index) {
          gridData.RowList[this.gridName][keyRow].Error = null;
        }
      }
    }
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
