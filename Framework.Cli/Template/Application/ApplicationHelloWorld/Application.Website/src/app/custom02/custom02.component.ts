import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: '[data-Custom02]',
  templateUrl: "./custom02.component.html",
  styleUrls: ['./custom02.component.scss']
})
export class Custom02Component implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  @Input() json: any;    
}