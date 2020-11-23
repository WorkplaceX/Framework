import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { CommandJson, DataService } from '../data.service';

@Component({
  selector: '[data-BulmaNavbar]',
  templateUrl: './bulma-navbar.component.html'
})
export class BulmaNavbarComponent implements OnInit {

  constructor(private dataService: DataService) { }

  @Input() 
  json: any

  @ViewChild('burger') 
  burger: ElementRef | undefined;

  @ViewChild('burgerTarget')
  burgerTarget: ElementRef | undefined;

  ngOnInit(): void {
  }

  clickHome() {
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 15, ComponentId: this.json.Id });
    return false;
  }

  clickBurger() {
    (<HTMLElement>this.burger?.nativeElement).classList.toggle("is-active");
    (<HTMLElement>this.burgerTarget?.nativeElement).classList.toggle("is-active");
  }

  click(navbarItem: any, event: MouseEvent | null) {
    if (event != null) {
      // Drop down does not close if level 1 item has a href attribute.
      (<HTMLElement>event.currentTarget).blur();
    }
    navbarItem.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 18, ComponentId: this.json.Id, BulmaNavbarItemId: navbarItem.Id });
    return false;
  }

  filterTextChange(navbarItem: any) {
    navbarItem.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 18, ComponentId: this.json.Id, BulmaNavbarItemId: navbarItem.Id, BulmaFilterText: navbarItem.FilterText });
  }

  trackBy(index: any, item: any) {
    return index; // or item.id
  }  
}
