import { Component, Input, OnInit } from '@angular/core';
import { CommandJson, DataService, Json } from '../data.service';

@Component({
  selector: '[data-BulmaNavbarMenu]',
  templateUrl: './bulma-navbar-menu.component.html',
  styles: [
  ]
})
export class BulmaNavbarMenuComponent implements OnInit {

  constructor(private dataService: DataService) { }

  @Input() 
  json!: Json

  ngOnInit(): void {
  }

  click(navbarItem: any) {
    navbarItem.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 22, ComponentId: this.json.Id, BulmaNavbarItemId: navbarItem.Id });
    return false;
  }

  trackBy(index: any, item: any) {
    return index; // or item.id
  }  

}
