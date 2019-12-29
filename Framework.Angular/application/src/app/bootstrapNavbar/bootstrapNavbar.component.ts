import { Component, Input } from "@angular/core";
import { DataService, RequestJson } from '../data.service';

/* BootstrapNavbar */
@Component({
  selector: '[data-BootstrapNavbar]',
  templateUrl: './bootstrapNavbar.component.html'
})
  export class BootstrapNavbar {
    constructor(dataService: DataService){
    this.dataService = dataService;
  }
  
  @Input() json: any
  dataService: DataService;
  
  click(button){
    button.IsShowSpinner = true;
    this.dataService.update(<RequestJson> { Command: 7, ComponentId: this.json.Id, BootstrapNavbarButtonId: button.Id });
  } 

  trackBy(index, item) {
    return index; // or item.id
  }  
}
  