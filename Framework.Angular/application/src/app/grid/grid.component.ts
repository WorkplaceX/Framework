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

  @Input() json: any;

  @ViewChild('inputFileUpload', {static: false}) inputFileUpload: ElementRef<HTMLElement>;

  ngModelChange(cell) {
    cell.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 9, ComponentId: this.json.Id, GridCellId: cell.Id, GridCellText: cell.Text });
  }

  clickSort(cell, event: MouseEvent) {
    if (this.resizeColumnIndex == null) {
      cell.IsShowSpinner = true;
      this.dataService.update(<CommandJson> { CommandEnum: 8, ComponentId: this.json.Id, GridCellId: cell.Id });
    }
  }

  clickConfig(cell, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    cell.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 12, ComponentId: this.json.Id, GridCellId: cell.Id });
  }

  private cellFileUpload: any;

  clickFileUpload(cell, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.cellFileUpload = cell;
    this.inputFileUpload.nativeElement.click();
  }

  changeInputFileUpload(files: FileList) {
    const file = files.item(0);

    const cellFileUpload = this.cellFileUpload;
    const dataService = this.dataService;
    const json = this.json;

    var reader = new FileReader();
    reader.readAsDataURL(file.slice()); 
    reader.onloadend = function() {
        var base64data = reader.result;                
        console.log(base64data);
        cellFileUpload.IsShowSpinner = true;
        dataService.update(<CommandJson> { CommandEnum: 9, ComponentId: json.Id, GridCellId: cellFileUpload.Id, GridCellText: cellFileUpload.Text, GridCellTextBase64: base64data, GridCellTextBase64FileName: file.name });
    }
  }  

  focus(cell) {
    if (!cell.IsSelect) {
      cell.IsShowSpinner = true;
      this.dataService.update(<CommandJson> { CommandEnum: 11, ComponentId: this.json.Id, GridCellId: cell.Id });
    }
  }

  focusout(cell) {
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
    this.json.CellList.forEach(cell => {
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

  resizeCell: any;
  resizeOffset: number;
  resizeColumnIndex: number;
  resizeColumnWidth: number;
  resizePageX: number;
  resizeTableWidth: number;

  click(event: MouseEvent) {
    event.stopPropagation(); // Prevent sort after column resize
  }

  mouseDown(cell, event: MouseEvent): boolean {
    event.stopPropagation();
    this.resizeCell = cell;
    let resizeCellElement = (<HTMLElement>event.currentTarget).parentElement.parentElement;
    this.resizeOffset = resizeCellElement.offsetWidth - event.pageX;
    return false;
  }

  mouseDown2(columnIndex, event: MouseEvent): boolean {
    event.stopPropagation();
    this.resizeColumnIndex = columnIndex;
    this.resizePageX = event.pageX;
    this.resizeColumnWidth = null;
    this.resizeTableWidth = (<HTMLElement>event.currentTarget).parentElement.parentElement.parentElement.parentElement.clientWidth;
    return false;
  }

  documentMouseMove(event: MouseEvent) {
    if (this.resizeCell != null) {
      this.resizeCell.Width = this.resizeOffset + event.pageX + 'px';
      this.json.StyleColumn = this.styleColumnList().join(" ");
    }
    if (this.resizeColumnIndex != null) {
      let resizeColumnWidth = this.json.StyleColumnWidthList[this.resizeColumnIndex];
      if (resizeColumnWidth.endsWith("%")) {
        let columnWidth = Number.parseInt(resizeColumnWidth.substring(0, resizeColumnWidth.length - 1));
        if (this.resizeColumnWidth == null) {
          this.resizeColumnWidth = columnWidth;
        }
        let offset = event.pageX - this.resizePageX;
        let offsetPercent = (offset / this.resizeTableWidth) * 100;
        let columnWidthNew = Math.round((this.resizeColumnWidth + offsetPercent) * 100) / 100;
        if (columnWidthNew < 0) {
          columnWidthNew = 0;
        }

        // ColumnWidthTotal
        let columnWidthTotal = 0;
        for (let i = 0; i < 3; i++)
        {

        }

        columnWidth = columnWidthNew;
        this.json.StyleColumnWidthList[this.resizeColumnIndex] = columnWidth + "%";

        console.log("columnWidthNew", columnWidthNew, "offset", offset, "columnWidth", columnWidth, "offsetPercent", offsetPercent);

      }
    }
  }

  documentMouseUp(event: MouseEvent) {
    if (this.resizeCell != null) {
      this.resizeCell = null;
      this.dataService.update(<CommandJson> { CommandEnum: 14, ComponentId: this.json.Id, GridStyleColumnList: this.styleColumnList() });
    }
    if (this.resizeColumnIndex != null) {
      this.resizeColumnIndex = null;
      event.stopPropagation();
    }
  }

  clickGrid(isClickEnum, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 10, ComponentId: this.json.Id, GridIsClickEnum: isClickEnum });
  }

  trackBy(index: any, item: any) {
    return index;
  }
}