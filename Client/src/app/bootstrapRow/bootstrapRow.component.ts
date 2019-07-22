import { Component, Input } from "@angular/core";
import { DataService } from "../../data.service";

/* BootstrapRow */
@Component({
  selector: '[data-BootstrapRow]',
  template: `
  <div [ngClass]="item.CssClass" data-Div [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class BootstrapRow {
  @Input() json: any
  
  trackBy(index, item) {
    return index; // or item.id
  }
}
  