import { Input, Component } from "@angular/core";
import { DataService, RequestJson } from '../data.service';

/* Grid */
@Component({
  selector: '[data-Grid]',
  templateUrl: './grid.component.html'
})
export class Grid {
  constructor(private dataService: DataService){
  }

  @Input() json: any

  ngModelChange(cell, row) {
    cell.IsShowSpinner = true;
    this.dataService.update(<RequestJson> { Command: 6, ComponentId: this.json.Id, GridRowId: row.Id, GridCellId: cell.Id, GridCellText: cell.Text });
  }

  focusoutCell: any;

  focusout(cell, row) {
    this.focusoutCell = cell;
  }

  focus(cell, row) {
    if (this.focusoutCell != null && this.focusoutCell.IsLookup) {
      // Close open loopup window
      this.focusoutCell.IsShowSpinner = true;
      this.dataService.update(<RequestJson> { Command: 4, ComponentId: this.json.Id, GridRowId: row.Id });
    } else {
      if (!row.IsSelect) {
        this.json.IsShowSpinner = true;
        this.dataService.update(<RequestJson> { Command: 4, ComponentId: this.json.Id, GridRowId: row.Id });
        }
    }
  }

  clickRow(row, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    if (!row.IsSelect && !this.json.IsShowSpinner) { // Prevent second POST after focus();
      this.json.IsShowSpinner = true;
      this.dataService.update(<RequestJson> { Command: 4, ComponentId: this.json.Id, GridRowId: row.Id });
    }
  }
  
  clickSort(column, event: MouseEvent) {
    if (this.resizeThPreventSort == false) {
      column.IsShowSpinned = true;
      this.dataService.update(<RequestJson> { Command: 2, ComponentId: this.json.Id, GridColumnId: column.Id });
    }
  }

  clickConfig(column, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    column.IsShowSpinned = true;
    this.dataService.update(<RequestJson> { Command: 3, ComponentId: this.json.Id, GridColumnId: column.Id });
  }

  clickGrid(isClickEnum, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.json.IsShowSpinner = true;
    this.dataService.update(<RequestJson> { Command: 5, ComponentId: this.json.Id, GridIsClickEnum: isClickEnum });
  }

  resizeTh: HTMLElement; // Resize table column see also: http://jsfiddle.net/thrilleratplay/epcybL4v/
  resizeThOffset: number;
  resizeThPreventSort: boolean = false; // Prevent sort after column resize
  
  thMouseDown(event: MouseEvent) {
    this.resizeTh = (<HTMLElement>event.srcElement).parentElement.parentElement;
    this.resizeThOffset = this.resizeTh.offsetWidth - event.pageX;
  }

  documentMouseMove(event: MouseEvent) {
    if (this.resizeTh != null) {
      this.resizeTh.style.width = this.resizeThOffset + event.pageX + 'px';
    } else {
      this.resizeThPreventSort = false;
    }
  }

  documentMouseUp(event: MouseEvent) {
    if (this.resizeTh != null) {
      this.resizeTh = null;
      this.resizeThPreventSort = true;
    }
  }

  trackBy(index, item) {
    return index; // or item.id
  }  
}