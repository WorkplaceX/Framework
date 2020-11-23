import { Component, ElementRef, Input, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { CommandJson, DataService } from '../data.service';

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <ng-container [ngSwitch]="json.Type">
    <div data-Page [ngClass]="json.CssClass" *ngSwitchCase="'Page'" [json]=json></div>
    <div data-Button style="display:inline" *ngSwitchCase="'Button'" [json]=json></div>
    <div data-Html style="display:inline" *ngSwitchCase="'Html'" [json]=json></div>
    <div data-Div [ngClass]="json.CssClass" *ngSwitchCase="'Div'" [json]=json></div>
    <div data-DivContainer [ngClass]="json.CssClass" *ngSwitchCase="'DivContainer'" [json]=json></div>
    <div data-BingMap [ngClass]="json.CssClass" *ngSwitchCase="'BingMap'" [json]=json></div>
    <div data-BulmaNavbar [ngClass]="json.CssClass" *ngSwitchCase="'BulmaNavbar'" [json]=json></div>
    <div data-BootstrapNavbar [ngClass]="json.CssClass" *ngSwitchCase="'BootstrapNavbar'" [json]=json></div>  
    <div data-Grid [ngClass]="json.CssClass" *ngSwitchCase="'Grid'" [json]=json></div>
  </ng-container>
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

  trackBy(index: any, item: any) {
    return item.TrackBy;
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
  constructor(private dataService: DataService){
  }

  @Input() json: any

  click(){
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 1, ComponentId: this.json.Id });
  } 
}

/* Html */
@Component({
  selector: '[data-Html]',
  template: `
  <div #div style="display:inline" [ngClass]="json.CssClass" [innerHtml]="textHtml" (click)="click($event);"></div>
  <i *ngIf="json.IsShowSpinner" class="fas fa-spinner fa-spin"></i>`
})
export class Html {
  @Input() json: any

  constructor(private dataService: DataService, private sanitizer: DomSanitizer){

  }

  textHtml: any;

  ngOnChanges() {
    if (this.json.IsNoSanatize) {
      this.textHtml = this.sanitizer.bypassSecurityTrustHtml(this.json.TextHtml);
    } else {
      this.textHtml = this.json.TextHtml;
    }
  }

  @ViewChild('div')
  div: ElementRef | undefined;

  click(event: MouseEvent){
    var element = event.target;
    do {
      if (element instanceof HTMLAnchorElement) {
        let anchor = <HTMLAnchorElement>element;
        if (anchor.classList.contains("navigatePost")) {
          event.preventDefault();
          this.json.IsShowSpinner = true;
          this.dataService.update(<CommandJson> { CommandEnum: 16, ComponentId: this.json.Id, NavigatePath: anchor.pathname });
        }
        break;
      }
      if (element instanceof HTMLButtonElement) {
        let button = <HTMLButtonElement>element;
        this.json.IsShowSpinner = true;
        this.dataService.update(<CommandJson> { CommandEnum: 19, ComponentId: this.json.Id, HtmlButtonId: button.id });
      }
      if (element instanceof HTMLElement) {
        element = (<HTMLElement>element).parentElement;
      } else {
        break;
      }
    } while (element != this.div?.nativeElement && element != null)
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
  
  trackBy(index: any, item: any) {
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
  map: ElementRef | undefined;
 
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