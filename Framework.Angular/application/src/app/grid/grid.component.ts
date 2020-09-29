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
    cell.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 8, ComponentId: this.json.Id, GridCellId: cell.Id });
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

  documentMouseMove(event: MouseEvent) {
    if (this.resizeCell != null) {
      
      this.resizeCell.Width = this.resizeOffset + event.pageX + 'px';
      this.json.StyleColumn = this.styleColumnList().join(" ");
    }
  }

  documentMouseUp(event: MouseEvent) {
    if (this.resizeCell != null) {
      this.resizeCell = null;
      this.dataService.update(<CommandJson> { CommandEnum: 14, ComponentId: this.json.Id, GridStyleColumnList: this.styleColumnList() });
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