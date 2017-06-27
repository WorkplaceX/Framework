import { Component } from '@angular/core';

@Component({
  selector: '[MyComponent]',
  template: '<p>Press the button:</p><button class="btn btn-primary" (click)="click()">{{t}}</button> <input type="text" />'
})
export class MyComponent {
  t: string = "Button";

  click(){
	this.t = this.t + ".";
  } 
}
