import { Component, OnInit, Input } from '@angular/core';
import { DataService, CommandJson } from '../data.service';

/* BootstrapNavbar */
@Component({
  selector: '[data-BootstrapNavbar]',
  templateUrl: './bootstrap-navbar.component.html'
})
export class BootstrapNavbarComponent implements OnInit {

  constructor(private dataService: DataService) { }

  ngOnInit(): void {
  }

  @Input() json: any

  click(button) {
    button.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 7, ComponentId: this.json.Id, BootstrapNavbarButtonId: button.Id });
    return false;
  }

  clickHome() {
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 15, ComponentId: this.json.Id });
    return false;
  }

  trackBy(index, item) {
    return index; // or item.id
  }  
}