import { Component, ElementRef, Input, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { CommandJson, DataService, Json } from '../data.service';

@Component({
  selector: '[data-BulmaNavbar]',
  templateUrl: './bulma-navbar.component.html'
})
export class BulmaNavbarComponent implements OnInit {

  constructor(private dataService: DataService) { }

  @Input() 
  json!: Json

  @ViewChild('burger') 
  burger: ElementRef | undefined;

  @ViewChild('burgerTarget')
  burgerTarget: ElementRef | undefined;

  @ViewChildren('dropDown') 
  dropDownList:QueryList<ElementRef> | undefined;

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

  click(navbarItem: any) {
    navbarItem.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 18, ComponentId: this.json.Id, BulmaNavbarItemId: navbarItem.Id });
    return false;
  }

  ngOnChanges() {
    // Called when after every dataService.update(); See also: @Input() json!: Json
    if (this.dropDownList != undefined) {
      let dropDownListLocal = this.dropDownList.toArray();
      // Close drop down
      dropDownListLocal.forEach(item => (<HTMLElement>item.nativeElement).classList.remove("is-hoverable"));
      setTimeout(() => {
        // Restore css class
        dropDownListLocal.forEach(item => (<HTMLElement>item.nativeElement).classList.add("is-hoverable"));
      }, 100);
    }
    // Close burger
    if (this.burger != undefined && this.burgerTarget != undefined) {
      (<HTMLElement>this.burger.nativeElement).classList.remove("is-active");
      (<HTMLElement>this.burgerTarget.nativeElement).classList.remove("is-active");
    }
  }  

  filterTextChange(navbarItem: any) {
    navbarItem.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 18, ComponentId: this.json.Id, BulmaNavbarItemId: navbarItem.Id, BulmaFilterText: navbarItem.FilterText });
  }

  trackBy(index: any, item: any) {
    return index; // or item.id
  }  
}
