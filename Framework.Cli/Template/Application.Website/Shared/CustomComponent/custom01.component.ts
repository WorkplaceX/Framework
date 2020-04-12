import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: '[data-custom01]',
  template: `
    <h1>Custom01 Component</h1>
    <p>
      TextHtml=(<span [innerHtml]="json.TextHtml"></span>);
    </p>
  `,
  styles: [
  ]
})
export class Custom01Component implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  @Input() json: any;    
}
