import { Input, Component } from "@angular/core";
import { DataService, RequestJson } from '../data.service';

/* Grid */
@Component({
  selector: '[data-Grid2]',
  templateUrl: './grid2.component.html'
})
export class Grid2 {
  constructor(private dataService: DataService){
  }

  @Input() json: any

  ngModelChange(cell) {
    cell.IsShowSpinner = true;
    this.dataService.update(<RequestJson> { Command: 9, ComponentId: this.json.Id, Grid2CellId: cell.Id, Grid2CellText: cell.Text });
  }

  clickSort(cell, event: MouseEvent) {
    this.dataService.update(<RequestJson> { Command: 8, ComponentId: this.json.Id, Grid2CellId: cell.Id });
  }

  clickConfig(cell, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    cell.IsShowSpinned = true;
    this.dataService.update(<RequestJson> { Command: 12, ComponentId: this.json.Id, Grid2CellId: cell.Id });
  }

  focus(cell) {
    if (cell.CellEnum==2 && !cell.IsSelect) {
      cell.IsShowSpinner = true;
      this.dataService.update(<RequestJson> { Command: 11, ComponentId: this.json.Id, Grid2CellId: cell.Id });
    }
  }

  focusout(cell) {
    if (cell.TextLeave != null) {
      if (cell.Text != cell.TextLeave) {
        cell.Text = cell.TextLeave;
        cell.TextLeave = null;
        cell.IsShowSpinner = true;
        this.dataService.update(<RequestJson> { Command: 13, ComponentId: this.json.Id, Grid2CellId: cell.Id });
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
    let resizeCellElement = (<HTMLElement>event.srcElement).parentElement.parentElement;
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
      this.dataService.update(<RequestJson> { Command: 14, ComponentId: this.json.Id, Grid2StyleColumnList: this.styleColumnList() });
    }
  }

  clickGrid(isClickEnum, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.json.IsShowSpinner = true;
    this.dataService.update(<RequestJson> { Command: 10, ComponentId: this.json.Id, GridIsClickEnum: isClickEnum });
  }

  trackBy(index: any, item: any) {
    return index;
  }
}