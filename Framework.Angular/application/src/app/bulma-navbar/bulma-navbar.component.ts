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
  burger: ElementRef;

  @ViewChild('burgerTarget')
  burgerTarget: ElementRef;

  ngOnInit(): void {
  }

  clickHome() {
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { Command: 15, ComponentId: this.json.Id });
    return false;
  }

  clickBurger() {
    (<HTMLElement>this.burger.nativeElement).classList.toggle("is-active");
    (<HTMLElement>this.burgerTarget.nativeElement).classList.toggle("is-active");
  }

  trackBy(index, item) {
    return index; // or item.id
  }  
}
