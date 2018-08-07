import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class Json {
  Name: string;

  Version: string;

  VersionBuild: string;

  IsServerSideRendering: boolean;

  RequestCount: number;

  RequestUrl: string;

  BrowserUrl: string;

  List: any;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public json: Json;

  public alertError: string; // Data service error.

  public alertInfo: string; // Data service info.

  public VersionBuild: string = "Build (local)";

  private isRequestPending: boolean = false; // Request is in prgress.

  constructor(private httpClient: HttpClient, @Inject('jsonServerSideRendering') private jsonServerSideRendering: any) { 
  if (jsonServerSideRendering != null) {
      this.json = jsonServerSideRendering;
      this.json.IsServerSideRendering = true;
    } else {
      this.json = jsonBrowser;
      this.json.IsServerSideRendering = false;
      if (window.location.href.startsWith("http://localhost:4200/")) {
        this.json.RequestUrl = "http://localhost:56092/";
        this.update();
      }
    }
  }

  update(): void {
    // RequestCount
    if (this.json.RequestCount == null) {
      this.json.RequestCount = 0;
    }
    this.json.RequestCount += 1;
    if (this.isRequestPending == false) { // Do not send a new request while old is still processing.
      this.isRequestPending = true;
      this.json.BrowserUrl = window.location.href;
      let requestUrl = new URL("/app.json", this.json.RequestUrl).href
      this.httpClient.post(requestUrl, JSON.stringify(this.json))
      .subscribe(body => {
        let jsonResponse = <Json>body;
        console.log(jsonResponse.RequestCount + " " + this.json.RequestCount);
        if (jsonResponse.RequestCount == this.json.RequestCount) { // Only apply response if there is no newer request.
          this.json = jsonResponse;
          this.isRequestPending = false;
        } else {
          this.isRequestPending = false;
          this.update(); // Process new request.
        }
        this.json.IsServerSideRendering = false;
      }, error => {
        console.log(error);
        this.alertError = "Request failed!";
      });
    }
  }
}
