import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: '[data-Custom01]',
  templateUrl: "./custom01.component.html",
  styles: [
  ]
})
export class Custom01Component implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  @Input() json: any;    
}