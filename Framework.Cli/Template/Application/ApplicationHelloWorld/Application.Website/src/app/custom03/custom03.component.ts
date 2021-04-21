import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: '[data-Custom03]',
  templateUrl: "./custom03.component.html",
  styles: [
  ]
})
export class Custom03Component implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  @Input() json: any;    
}