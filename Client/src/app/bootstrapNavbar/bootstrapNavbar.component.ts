import { Component, Input } from "@angular/core";
import { DataService } from "../../data.service";

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
  
    click(){
      this.dataService.update();
    } 
  }
  