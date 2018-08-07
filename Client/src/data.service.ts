import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class Json {
  Name: string;

  Version: string;

  VersionBuild: string;

  IsServerSideRendering: boolean;

  RequestUrl: string;

  BrowserUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public json: Json;

  public VersionBuild: string = "Build (local)";

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
    this.json.BrowserUrl = window.location.href;
    let requestUrl = new URL("/app.json", this.json.RequestUrl).href
    this.httpClient.post(requestUrl, JSON.stringify(this.json))
    .subscribe(body => {
      this.json = <Json>body;
      this.json.IsServerSideRendering = false;
    });
  }
}
