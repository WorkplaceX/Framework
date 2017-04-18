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
    return 0;
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
  <InputX *ngIf="json.Type=='Input'" [json]=json></InputX>
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
  <div style='background-color:#F2F5A9;' class='container' removeSelector>
    Text={{ json.Text }}
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutContainer {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Type;
  }
}

/* LayoutRow */
@Component({
  selector: 'LayoutRow',
  template: `
  <div style='background-color:#F6D8CE;' class='row' removeSelector>
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutRow {
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Type;
  }
}

/* LayoutCell */
@Component({
  selector: 'LayoutCell',
  template: `
  <div style='background-color:#CEF6CE;' [class.col-sm-6]='true' removeSelector>
    <p>
    Text={{ json.Text }}
    </p>
    <Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></Selector>
  </div>  
`
})
export class LayoutCell {
  @Input() json: any

  trackBy(index: any, item: any): any {
    return item.Type;
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

  trackBy(index: any, item: any): any {
    return item.Type;
  }
}

/* Button */
@Component({
  selector: 'ButtonX',
  template: `<button type="text" class="btn btn-primary" (click)="click()">{{ json.Text }}</button>`
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
  template: `<div [innerHTML]=json.Text></div>`
})
export class Literal {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @Input() json: any
  dataService: DataService;
}

/* InputX */
@Component({
  selector: 'InputX',
  template: `
  <input type="text" class="form-control" [(ngModel)]="text" (ngModelChange)="onChange()" (focus)="focus(true)" (focusout)="focus(false)" placeholder="Empty"/>
  <p>
    Text={{ json.Text }}<br/>
    TextNew={{ json.TextNew}}<br/>
    Focus={{json.IsFocus}}<br/>
    AutoComplete={{json.AutoComplete}}
  </p>`
})
export class InputX {
  @Input() json: any
  dataService: DataService;
  text: string;
  inputFocused: any;

  constructor( dataService: DataService){
    this.dataService = dataService;
  }

  ngOnInit() {
    this.text = this.json.Text;
  }  

  onChange() {
    this.json.TextNew = this.text;
    this.dataService.update();
  }

  onKey(event:any) {
    this.text = event.target.value;
    this.json.TextNew = this.text;
    this.dataService.update();
  }

  focus(isFocus: boolean) {
    this.json.IsFocus = isFocus;
  }  
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
  <div style="white-space: nowrap;">
  <GridHeader [json]=item *ngFor="let item of dataService.json.GridData.ColumnList[json.GridName]; trackBy trackBy"></GridHeader>
  </div>
  <GridRow [jsonGridData]=dataService.json.GridData [jsonGrid]=json [json]=item *ngFor="let item of dataService.json.GridData.RowList[json.GridName]; trackBy trackBy"></GridRow>
  `
})
export class Grid {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  dataService: DataService;
  @Input() json: any

  trackBy(index: any, item: any) {
    return item.Type;
  }
}

/* GridRow */
@Component({
  selector: 'GridRow',
  template: `
  <div (click)="click()" (mouseover)="mouseOver()" (mouseout)="mouseOut()" [ngClass]="{'select-class1':json.IsSelect==1, 'select-class2':json.IsSelect==2, 'select-class3':json.IsSelect==3}" style="white-space: nowrap;">
  <GridCell [jsonGrid]=jsonGrid [jsonGridData]=jsonGridData [jsonRow]=json [json]=item *ngFor="let item of jsonGridData.ColumnList[jsonGrid.GridName]; trackBy trackBy"></GridCell>
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
  @Input() jsonGridData: any;
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
    return item.Type;
  }

  click(){
    this.json.IsClick = true;
    this.dataService.update();
  }
}

/* GridCell */
@Component({
  selector: 'GridCell',
  template: `
  <div (click)="click($event)" [ngClass]="{'select-class':jsonGridData.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].IsSelect}" style="display:inline-block; position:relative;" [style.width.%]=json.WidthPercent>
  <div style='margin-right:30px;text-overflow: ellipsis; overflow:hidden;'>
  {{ jsonGridData.CellList[jsonGrid.GridName][json.FieldName][jsonRow.Index].V }}
  <img src='ArrowDown.png' style="width:12px;height:12px;top:8px;position:absolute;right:7px;"/>
  </div>
  <GridField [gridName]=jsonGrid.GridName [fieldName]=json.FieldName [index]=jsonRow.Index></GridField>
  `,
  styles: [`
  .select-class {
    border:solid 2px blue;
  }
  `]
})
export class GridCell {
  @Input() json: any; // Column // Used for FieldName
  @Input() jsonRow: any; // Used for Index
  @Input() jsonGrid: any; // Used for GridName
  @Input() jsonGridData: any; // Used for Value
  dataService: DataService;

  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  trackBy(index: any, item: any) {
    return item.Type;
  }

  click(event: MouseEvent){
    this.jsonGridData.CellList[this.jsonGrid.GridName][this.json.FieldName][this.jsonRow.Index].IsClick = true;
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
  @Input() json: any; // Column

  trackBy(index: any, item: any) {
    return item.Type;
  }

  click(){
    this.json.IsSelect = !this.json.IsSelect;
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
  <input type="text" class="form-control" [(ngModel)]="gridCell().V" (ngModelChange)="onChange()" (focus)="focus(true)" (focusout)="focus(false)" [focus]="dataService.json.GridData.FocusIndex==index && dataService.json.GridData.FocusFieldName == fieldName" placeholder="Empty" />
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
    let gridData: any = this.dataService.json.GridData;
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

  onChange() {
    let isUpdate = false;
    let point = this.point();
    if (point.gridName != null) { 
      // GridName can be null if no focus is set.
      for (let column of point.gridData.ColumnList[point.gridName]) {
        if (column.FieldName == point.fieldName) {
          isUpdate = column.IsUpdate;
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
    let gridData: any = this.dataService.json.GridData;
    // GridName
    for (let keyGridLoad in gridData.GridLoadList) {
      let gridName = gridData.GridLoadList[keyGridLoad].GridName;
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
    var gridData: any = this.dataService.json.GridData;
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
