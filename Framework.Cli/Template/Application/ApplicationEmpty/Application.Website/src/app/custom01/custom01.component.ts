import { Component, OnInit, Input, ViewChild, ElementRef, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: '[data-Custom01]',
  template: `
    <h1>Custom01 Component</h1>
    <p>
      TextHtml=(<span [innerHtml]="json.TextHtml"></span>);
    </p>
    <input type="text" placeholder="NtpServer" [(ngModel)]="ntpServer">
    <p>
    Offset={{ offset }}; TimeResult={{ timeResult }};
    </p>
    <canvas #canvas width="300" height="300"></canvas>
  `,
  styles: [
    'canvas { border-style: solid }'
  ]
})
export class Custom01Component implements OnInit {

  constructor(private http: HttpClient, private ngZone: NgZone) { }

  ntpServer: string = "";
  offset: number = 0;
  timeResult: string | undefined;

  ctx!: CanvasRenderingContext2D;

  @ViewChild('canvas', { static: true })
  canvas!: ElementRef<HTMLCanvasElement>;

  ngOnInit(): void {
    this.ctx = this.canvas.nativeElement.getContext('2d') as any;
    this.ngZone.runOutsideAngular(() => this.tick());
    setInterval(() =>{
      this.http.get("http://localhost:5721/?command=trigger&ntpServer=" + this.ntpServer, {responseType: 'text'}).subscribe((data) => {
        this.timeResult = data;
        let timeResult = <TimeResult>JSON.parse(data);
        this.offset = timeResult.Offset;
      });
    }, 10000);
  }

  tick() {
    this.ctx.clearRect(0, 0, this.ctx.canvas.width, this.ctx.canvas.height);

    let date = new Date();
    let timeMillisecond = date.getTime();

    timeMillisecond += this.offset;

    let dateNtp = new Date(timeMillisecond);

    let pixel = ((timeMillisecond % 1000) / 1000) * this.ctx.canvas.height;

    this.ctx.fillStyle = 'lightblue';
    this.ctx.fillRect(0, 0, this.ctx.canvas.width, pixel);

    this.ctx.fillStyle = 'black';
    this.ctx.font = "30px Arial";
    this.ctx.fillText("Local=" + date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds(), 10, 50);
    this.ctx.fillText("NTP=" + date.getHours() + ":" + dateNtp.getMinutes() + ":" + dateNtp.getSeconds(), 10, 100);

    window.requestAnimationFrame(() => this.tick());
  }

  @Input() json: any;    
}

class TimeResult
{
  Offset!: number;
}