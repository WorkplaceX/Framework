import { Component, OnInit, ViewChild, Input, ElementRef } from '@angular/core';
import { DataService, CommandJson } from '../data.service';

/* Grid */
@Component({
  selector: '[data-Grid]',
  templateUrl: './grid.component.html',
  styles: [
  ]
})
export class GridComponent implements OnInit {

  constructor(private dataService: DataService) { }

  ngOnInit(): void {
  }

  ngAfterViewInit() {
    (<HTMLTableElement>this.table?.nativeElement).addEventListener("wheel", event => this.wheel(event));
    if (this.divGridClick != null) { // Does not exist if IsHidePagination
      (<HTMLDivElement>this.divGridClick.nativeElement).addEventListener("wheel", event => this.wheel(event));
    }
  }

  @Input() json: any;

  @ViewChild('table')
  table: ElementRef | undefined;

  @ViewChild('divGridClick')
  divGridClick: ElementRef | undefined;

  @ViewChild('inputFileUpload', {static: false}) 
  inputFileUpload: ElementRef<HTMLElement> | undefined;

  ngModelChange(cell: any) {
    cell.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 9, ComponentId: this.json.Id, GridCellId: cell.Id, GridCellText: cell.Text });
  }

  clickSort(cell: any, event: MouseEvent) {
    cell.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 8, ComponentId: this.json.Id, GridCellId: cell.Id });
  }

  clickConfig(cell: any, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    cell.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 12, ComponentId: this.json.Id, GridCellId: cell.Id });
  }

  private cellFileUpload: any;

  clickFileUpload(cell: any, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.cellFileUpload = cell;
    this.inputFileUpload?.nativeElement.click();
  }

  changeInputFileUpload(event: Event) {
    const file = (<HTMLInputElement>event.target).files?.item(0);
    (<HTMLInputElement>event.target).value = ""; // Upload same file multiple times. Trigger change event.

    const cellFileUpload = this.cellFileUpload;
    const dataService = this.dataService;
    const json = this.json;

    var reader = new FileReader();
    if (file != null){
      reader.readAsDataURL(file.slice()); 
    }
    reader.onloadend = function() {
        var base64data = reader.result;
        cellFileUpload.IsShowSpinner = true;
        dataService.update(<CommandJson> { CommandEnum: 9, ComponentId: json.Id, GridCellId: cellFileUpload.Id, GridCellText: cellFileUpload.Text, GridCellTextBase64: base64data, GridCellTextBase64FileName: file?.name });
    }
  }

  clickCell(cell: any, event: MouseEvent) {
    if (!(event.target instanceof HTMLInputElement)) {
      this.focus(cell);
    }
  }

  focus(cell: any) {
    if (!cell.IsSelect) {
      cell.IsShowSpinner = true;
      this.dataService.update(<CommandJson> { CommandEnum: 11, ComponentId: this.json.Id, GridCellId: cell.Id });
    }
  }

  focusout(cell: any) {
    if (cell.TextLeave != null) {
      if (cell.Text != cell.TextLeave) {
        cell.Text = cell.TextLeave;
        cell.TextLeave = null;
        cell.IsShowSpinner = true;
        this.dataService.update(<CommandJson> { CommandEnum: 13, ComponentId: this.json.Id, GridCellId: cell.Id });
      }
    }
  }

  styleColumnList(): string[] {
    let result: string[] = [];
    this.json.CellList.forEach((cell: { CellEnum: number; Width: string | null; }) => {
      if (cell.CellEnum == 4){
        if (cell.Width == null) {
          result.push("minmax(0, 1fr)");
        }
        else {
          result.push(cell.Width);
        }
      }
    });
    return result;
  }

  resizeColumnIndex: number | undefined; // If not null, user column resize in progress
  resizeColumnWidthValue: number | undefined;
  resizePageX: number | undefined;
  resizeTableWidth: number | undefined;

  click(event: MouseEvent) {
    event.stopPropagation(); // Prevent sort after column resize
  }

  mouseDown(columnIndex: number, event: MouseEvent): boolean {
    event.stopPropagation();
    this.resizeColumnIndex = columnIndex;
    this.resizePageX = event.pageX;
    this.resizeColumnWidthValue = undefined;
    this.resizeTableWidth = (<HTMLElement>event.currentTarget).parentElement?.parentElement?.parentElement?.parentElement?.clientWidth;
    return false;
  }    

  documentMouseMove(event: MouseEvent) {
    if (this.resizeColumnIndex != null) {
      let styleColumn = this.json.StyleColumnList[this.resizeColumnIndex];
      let widthValue = styleColumn.WidthValue;
      if (this.resizeColumnWidthValue == null) {
        this.resizeColumnWidthValue = widthValue;
      }
      let offset = event.pageX - (this.resizePageX || 0);
      let offsetPercent = (offset / (this.resizeTableWidth || 0)) * 100;
      let columnWidthNew = Math.round(((this.resizeColumnWidthValue || 0) + offsetPercent) * 100) / 100;
      if (columnWidthNew < 0) {
        columnWidthNew = 0;
      }
      
      // ColumnWidthTotal
      let columnWidthTotal = 0;
      for (let i = 0; i < this.json.StyleColumnList.length; i++) {
        let widthValue = this.json.StyleColumnList[i].WidthValue;
        if (i != this.resizeColumnIndex && widthValue != null) {
          columnWidthTotal += widthValue;
        }
      }
      if (columnWidthTotal + columnWidthNew > 100) {
        columnWidthNew = 100 - columnWidthTotal;
      }

      widthValue = columnWidthNew;
      styleColumn.Width = widthValue + styleColumn.WidthUnit;
      styleColumn.WidthValue = widthValue;
    }
  }

  documentMouseUp(event: MouseEvent) {
    if (this.resizeColumnIndex != null) {
      event.stopPropagation();
      let resizeColumnIndexLocal = this.resizeColumnIndex;
      this.resizeColumnIndex = undefined;
      let widthValue = <number>this.json.StyleColumnList[resizeColumnIndexLocal].WidthValue;
      this.dataService.update(<CommandJson> { CommandEnum: 20, ComponentId: this.json.Id, ResizeColumnIndex: resizeColumnIndexLocal, ResizeColumnWidthValue: widthValue });
    }
  }

  clickGrid(isClickEnum: number, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 10, ComponentId: this.json.Id, GridIsClickEnum: isClickEnum });
  }

  wheel(event: WheelEvent) {

    if (event.altKey) {
      let gridIsClickEnum = 0;

      if (!event.shiftKey && event.deltaY > 0) {
        gridIsClickEnum = 2; // PageDown
      }
      if (!event.shiftKey && event.deltaY < 0) {
        gridIsClickEnum = 1; // PageUp
      }
      if (event.shiftKey && event.deltaY > 0) {
        gridIsClickEnum = 4; // PageRight
      }
      if (event.shiftKey && event.deltaY < 0) {
        gridIsClickEnum = 3; // PageLeft
      }

      if (gridIsClickEnum != 0) {
        this.json.IsShowSpinner = true;
        this.dataService.update(<CommandJson> { CommandEnum: 10, ComponentId: this.json.Id, GridIsClickEnum: gridIsClickEnum });
        return true;
      }
    }

    return false;
  }

  trackBy(index: any, item: any) {
    return index;
  }
}