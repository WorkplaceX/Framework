import { Component, OnInit, Input, ViewChild, ElementRef, SimpleChanges } from '@angular/core';
import { DataService, RequestJson } from '../data.service';

@Component({
  selector: 'app-framework',
  template: `
    <p>
      framework works!
    </p>
  `,
  styles: [
  ]
})
export class FrameworkComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <div data-Button style="display:inline" *ngIf="json.Type=='Button'" [json]=json></div>
  <div data-Div [ngClass]="json.CssClass" *ngIf="json.Type=='Div'" [json]=json></div>
  <div data-DivContainer [ngClass]="json.CssClass" *ngIf="json.Type=='DivContainer'" [json]=json></div>
  <div data-Page [ngClass]="json.CssClass" *ngIf="json.Type=='Page' && !json.IsHide" [json]=json></div>
  <div data-Html style="display:inline" *ngIf="json.Type=='Html'" [json]=json></div>
  <div data-Grid [ngClass]="json.CssClass" *ngIf="json.Type=='Grid' && !json.IsHide" [json]=json></div>
  <div data-BootstrapNavbar [ngClass]="json.CssClass" *ngIf="json.Type=='BootstrapNavbar'" [json]=json></div>  
  <div data-BingMap [ngClass]="json.CssClass" *ngIf="json.Type=='BingMap'" [json]=json></div>
  <div data-custom01 style="display:inline" *ngIf="json.Type=='Custom01'" [json]=json></div>

  `
})
export class Selector {
  @Input() json: any
}

/* Page */
@Component({
  selector: '[data-Page]',
  template: `
  <div style="display:inline" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class Page {
  @Input() json: any
  dataService: DataService;

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* Html */
@Component({
  selector: '[data-Html]',
  template: `<i *ngIf="json.IsShowSpinner" class="fas fa-spinner fa-spin"></i>  
  <div #div style="display:inline" [ngClass]="json.CssClass" [innerHtml]="json.TextHtml" (click)="click($event);" ></div>`
})
export class Html {
  @Input() json: any

  constructor(private dataService: DataService){
  }

  @ViewChild('div')
  div: ElementRef;

  click(event: MouseEvent){
    this.json.IsShowSpinner = true;
    var element = event.srcElement;
    do {
      if (element instanceof HTMLAnchorElement) {
        var anchor = <HTMLAnchorElement>element;
        if (anchor.classList.contains("linkPost")) {
          event.preventDefault();
          this.dataService.update(<RequestJson> { Command: 16, ComponentId: this.json.Id, LinkPostPath: anchor.pathname });
        }
        break;
      }
      if (element instanceof HTMLElement) {
        element = (<HTMLElement>element).parentElement;
      } else {
        break;
      }
    } while (element != this.div.nativeElement && element != null)
  } 
}

/* Button */
@Component({
  selector: '[data-Button]',
  template: `
  <button [ngClass]="json.CssClass" (click)="click();" [innerHtml]="json.TextHtml"></button>
  <i *ngIf="json.IsShowSpinner" class="fas fa-spinner fa-spin"></i>  
  `
})
export class Button {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @Input() json: any
  dataService: DataService;

  click(){
    this.json.IsShowSpinner = true;
    this.dataService.update(<RequestJson> { Command: 1, ComponentId: this.json.Id });
  } 
}

/* Div */
@Component({
  selector: '[data-Div]',
  template: `
  <div style="display:inline" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class Div {
  @Input() json: any;

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* DivContainer */
@Component({
  selector: '[data-DivContainer]',
  template: `
    <div [ngClass]="item.CssClass" data-Div [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class DivContainer {
  @Input() json: any;
  
  trackBy(index, item) {
    return index; // or item.id
  }
}

/* BingMap */
declare var scriptBingMap: any;
@Component({
  selector: '[data-BingMap]',
  template: `
  <div #map id="myMap" style="height:400px;"></div>
  <script></script>
  `
})
export class BingMap {
  constructor(dataService: DataService){
    this.dataService = dataService;
  }

  @Input() json: any;
  dataService: DataService;

  @ViewChild('map', { static: true}) 
  map: ElementRef;
 
  ngOnChanges(changes: SimpleChanges) {
    if (changes.json.previousValue == null || changes.json.previousValue.Lat != changes.json.currentValue.Lat || changes.json.previousValue.Long != changes.json.currentValue.Long)
    {
      if (this.dataService.json.IsServerSideRendering == false) {
        this.scriptBingMapInit();
        scriptBingMap({ Lat: changes.json.currentValue.Lat, Long: changes.json.currentValue.Long});
      }
    }
  }

  scriptBingMapInit() {
    if (this.dataService.document.getElementById('scriptBingMap')) {
      // scriptBingMap is defined
      return;
    }

    const script = this.dataService.document.createElement('script');
    script.id = "scriptBingMap";
    script.text = `
    scriptBingMapIsInit = false;
    scriptBingMapPos = null;
    function scriptBingMap(pos) {
      if (!scriptBingMapIsInit && pos == null) {
        scriptBingMapIsInit = true;
      }
      if (pos != null) {
        scriptBingMapPos = pos;
      }
      if (pos == null && scriptBingMapPos != null) {
        pos = scriptBingMapPos;
      }

      if (scriptBingMapIsInit) {
        var map = new Microsoft.Maps.Map(document.getElementById('myMap'), {});
        map.setView({
            center: new Microsoft.Maps.Location(pos.Lat, pos.Long),
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,            
            zoom: 15
        });
				var pushpin = new Microsoft.Maps.Pushpin(map.getCenter(), null);
        map.entities.push(pushpin);
      }
    }
    `
    this.dataService.renderer.appendChild(this.dataService.document.head, script);

    const scriptApi = this.dataService.document.createElement('script');
    
    scriptApi.src = 'https://www.bing.com/api/maps/mapcontrol?key=' + this.json.Key + '&callback=scriptBingMap';
    scriptApi.async = true;
    scriptApi.defer = true;
    this.dataService.renderer.appendChild(this.dataService.document.head, scriptApi);
  }
}