import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable, Subject } from 'rxjs';
import { bufferTime } from 'rxjs/operators';
// import { of, interval } from 'rxjs';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class Json {
  Name: string;

  Version: string;

  VersionBuild: string;

  IsServerSideRendering: boolean;

  Session: string;

  SessionApp: string;

  SessionState: string;

  RequestCount: number;

  RequestUrl: string;

  EmbeddedUrl: string;

  BrowserUrl: string;

  IsReload: boolean;

  List: any;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public json: Json = new Json();

  public alertError: Subject<string> = new Subject<string>(); // Data service error.

  public alertErrorList$: Observable<string[]>;

  public VersionBuild: string = "Build (local)";

  public isRequestPending: boolean = false; // Request is in prgress.

  public mergeCount: number = 0; // Make cell merge ready on first modify.

  public mergeBufferId: number;

  public mergeBufferText: string; // Buffer text entered during pending request.

  constructor(private httpClient: HttpClient, @Inject('jsonServerSideRendering') private jsonServerSideRendering: any) { 
    setTimeout(() => {
      this.alertError.next("Init"); // Does not show on startup without setTimeout!
    }, 0);

    if (this.jsonServerSideRendering != null) {
      this.json = this.jsonServerSideRendering;
      this.json.IsServerSideRendering = true;
    } else {
      this.json = jsonBrowser;
      this.json.IsServerSideRendering = false;
      this.alertErrorList$ = this.alertError.pipe(bufferTime(2000, 1, 1)); // No async during server side rendering!
      if (window.location.href.startsWith("http://localhost:4200/")) { // Running in Framework\Client\
        this.json.RequestUrl = "https://localhost:44334/";
        this.update();
      } 
      if (window != null) { // Running on client.
        this.json.RequestUrl = window.location.href;
        this.update();
      }
    }
  }

  // Merge text entered by user during pending request.
  private merge(json: any): void { 
    if (this.mergeBufferId != null) {
      if (json.MergeId == this.mergeBufferId) {
        json.Text = this.mergeBufferText;
        json.IsModify = true;
        json.IsClick = true; // Show spinner
        this.mergeBufferId = null;
        this.mergeBufferText = null;
      }
      for (const key in json) {
        let item = json[key];
        if (item != null && typeof(item) == 'object') {
            this.merge(item);
        }
      }
    }
  }

  public update(): void {
    // RequestCount
    if (this.json.RequestCount == null) {
      this.json.RequestCount = 0;
    }
    this.json.RequestCount += 1;
    if (this.isRequestPending == false) { // Do not send a new request while old is still processing.
      this.isRequestPending = true;
      this.json.BrowserUrl = window.location.href;
      let requestUrl;
      if (this.json.EmbeddedUrl != null) {
        requestUrl = new URL("/app.json", this.json.EmbeddedUrl).href 
      } else {
        requestUrl = new URL("/app.json", this.json.RequestUrl).href 
      }

      this.httpClient.request("POST", requestUrl, {
        body: JSON.stringify(this.json),
        withCredentials: true,
      })
      .subscribe(body => {
        let jsonResponse = <Json>body;
        if (jsonResponse.RequestCount == this.json.RequestCount) { // Apply response if there is no newer request.
          this.json = jsonResponse;
          this.isRequestPending = false;
        } else {
          this.merge(jsonResponse);
          this.json = jsonResponse;
          this.isRequestPending = false;
          this.update(); // Process new merged request.
        }
        this.json.IsServerSideRendering = false;
        if (this.json.IsReload) {
          location.reload(true);
        }
      }, error => {
        this.isRequestPending = false;
        this.alertError.next("Request failed!");
      });
    }
  }
}
