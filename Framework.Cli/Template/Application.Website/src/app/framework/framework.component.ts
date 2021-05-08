import { Component, ElementRef, Input, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { CommandJson, DataService, Json } from '../data.service';

/* Selector */
@Component({
  selector: '[data-Selector]',
  template: `
  <ng-container [ngSwitch]="json.Type">
    <div data-Page [ngClass]="json.CssClass!" *ngSwitchCase="'Page'" [json]=json></div>
    <div data-Button style="display:inline" *ngSwitchCase="'Button'" [json]=json></div>
    <div data-Html style="display:inline" *ngSwitchCase="'Html'" [json]=json></div>
    <div data-Div [ngClass]="json.CssClass!" *ngSwitchCase="'Div'" [json]=json></div>
    <div data-DivContainer [ngClass]="json.CssClass!" *ngSwitchCase="'DivContainer'" [json]=json></div>
    <div data-BingMap [ngClass]="json.CssClass!" *ngSwitchCase="'BingMap'" [json]=json></div>
    <div data-Dialpad *ngSwitchCase="'Dialpad'" [json]=json></div>
    <div data-BulmaNavbar [ngClass]="json.CssClass!" *ngSwitchCase="'BulmaNavbar'" [json]=json></div>
    <div data-BootstrapNavbar [ngClass]="json.CssClass!" *ngSwitchCase="'BootstrapNavbar'" [json]=json></div>  
    <div data-Grid [ngClass]="json.CssClass!" *ngSwitchCase="'Grid'" [json]=json></div>
    <div data-Custom01 *ngSwitchCase="'Custom01'" [json]=json></div>
    <div data-Custom02 *ngSwitchCase="'Custom02'" [json]=json></div>
    <div data-Custom03 *ngSwitchCase="'Custom03'" [json]=json></div>
  </ng-container>
  `
})
export class Selector {
  @Input() json!: Json
}

/* Page */
@Component({
  selector: '[data-Page]',
  template: `
  <div style="display:inline" data-Selector [json]=item *ngFor="let item of json.List; trackBy trackBy"></div>
  `
})
export class Page {
  @Input() json!: Json

  trackBy(index: any, item: any) {
    return item.TrackBy;
  }
}

/* Button */
@Component({
  selector: '[data-Button]',
  template: `
  <button [ngClass]="json.CssClass!" (click)="click();" [innerHtml]="json.TextHtml"></button>
  <i *ngIf="json.IsShowSpinner" class="fas fa-spinner fa-spin"></i>  
  `
})
export class Button {
  constructor(private dataService: DataService){
  }

  @Input() json!: Json

  click(){
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 1, ComponentId: this.json.Id });
  } 
}

/* Html */
@Component({
  selector: '[data-Html]',
  template: `
  <div #div style="display:inline" [ngClass]="json.CssClass!" [innerHtml]="textHtml" (click)="click($event);"></div>
  <i *ngIf="json.IsShowSpinner" class="fas fa-spinner fa-spin"></i>`
})
export class Html {
  @Input() json!: Json

  constructor(private dataService: DataService, private sanitizer: DomSanitizer){

  }

  textHtml: SafeHtml | undefined;

  textHtmlPrevious: any = false; // Detect TextHtml change. Also if text is null on first change (false)!

  isNoSanatizePrevious: boolean | undefined; // Detect IsNoSanatize change.

  ngOnChanges() {
    if (this.json.IsNoSanatize) {
      if (this.textHtmlPrevious != this.json.TextHtml || this.isNoSanatizePrevious != this.json.IsNoSanatize) { // Change detection
        this.textHtmlPrevious = this.json.TextHtml;
        this.textHtml = this.sanitizer.bypassSecurityTrustHtml(this.json.TextHtml ? this.json.TextHtml : "");
        if (this.json.IsNoSanatizeScript != null) {
          setTimeout(() => eval(<string>this.json.IsNoSanatizeScript), 0);
        }
      }
    } else {
      this.textHtml = this.json.TextHtml;
    }
    this.isNoSanatizePrevious = this.json.IsNoSanatize;
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
  @Input() json!: Json;

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
  @Input() json!: Json;
  
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

  @Input() json!: Json;
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

/* Dialpad */
@Component({
  selector: '[data-Dialpad]',
  template: `
  <div class='dialpad'>
    <div class='row'>
      <button [ngClass]="json.CssClass!" (click)="click('1');">1<sub>&nbsp;</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('2');">2<sub>(ABC)</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('3');">3<sub>(DEF)</sub></button>
    </div>
    <div class='row'>
      <button [ngClass]="json.CssClass!" (click)="click('4');">4<sub>(GHI)</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('5');">5<sub>(JKL)</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('6');">6<sub>(MNO)</sub></button>
    </div>
    <div class='row'>
      <button [ngClass]="json.CssClass!" (click)="click('7');">7<sub>(PQRS)</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('8');">8<sub>(TUV)</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('9');">9<sub>(WXYZ)</sub></button>
    </div>
    <div class='row'>
      <button [ngClass]="json.CssClass!" (click)="click('*');">*<sub>&nbsp;</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('0');">0<sub>(+)</sub></button>
      <button [ngClass]="json.CssClass!" (click)="click('#');">#<sub>&nbsp;</sub></button>
    </div>
    <i *ngIf="json.IsShowSpinner" class="fas fa-spinner fa-spin"></i>
  </div>
  `
})
export class Dialpad {
  constructor(private dataService: DataService){
  }

  @Input() json!: Json

  click(text: string){
    this.json.IsShowSpinner = true;
    this.dataService.update(<CommandJson> { CommandEnum: 21, ComponentId: this.json.Id, DialpadText: text });
  } 
}
