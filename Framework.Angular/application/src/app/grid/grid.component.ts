import { Input, Component } from "@angular/core";
import { DataService, JsonRequest } from '../data.service';

/* Grid */
@Component({
  selector: '[data-Grid]',
  templateUrl: './grid.component.html'
})
export class Grid {
  constructor(private dataService: DataService){
  }

  @Input() json: any

  ngModelChange(cell) {
    cell.IsModify = true;
    cell.IsClick = true; // Show spinner

    // Merge
    if (cell.MergeId == null) {
      this.dataService.mergeCount += 1;
      cell.MergeId = this.dataService.mergeCount; // Make cell "merge ready".
    }
    if (this.dataService.isRequestPending == true) {
      this.dataService.mergeBufferId = cell.MergeId;
      this.dataService.mergeBufferText = cell.Text; // Buffer user input during pending request.
    }

    this.dataService.update(null);
  }

  focus(row) {
    if (!row.IsSelect && !row.IsClick) {
      row.IsClick = true;
      this.dataService.update(null);
    }
  }

  clickRow(row, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    if (!row.IsSelect && !row.IsClick) {
      row.IsClick = true;
      this.dataService.update(null);
    }
  }
  
  clickSort(param, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    param.column.IsClickSort = true;
    this.dataService.update(<JsonRequest> { Id: this.json.Id, ColumnId: param.columnId });
  }

  clickConfig(column, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    column.IsClickConfig = true;
    this.dataService.update(null);
  }

  clickGrid(isClickEnum, event: MouseEvent) {
    event.stopPropagation(); // Prevent underlying Grid to fire click event. (Lookup grid)
    this.json.IsClickEnum = isClickEnum;
    this.dataService.update(null);
  }


  trackBy(index, item) {
    return index; // or item.id
  }  
}