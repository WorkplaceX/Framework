import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class RequestJson {
  Command: number;

  ComponentId: number;

  GridColumnId: number;

  GridRowId: number;

  GridCellId: number;

  GridCellText: string;

  GridIsClickEnum : number;

  RequestCount: number;

  IsRequestJson: boolean;
}

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

  IsBootstrapModal: boolean;

  List: any;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public json: Json = new Json();
  
  public VersionBuild: string = "Build (local)"; /* VersionBuild */
  
  public isRequestPending: boolean = false; // Request is in prgress.

  public mergeCount: number = 0; // Make cell merge ready on first modify.

  public mergeBufferId: number;
  
  public mergeBufferText: string; // Buffer text entered during pending request.

  constructor(private httpClient: HttpClient, @Inject('jsonServerSideRendering') private jsonServerSideRendering: any) { 
    if (this.jsonServerSideRendering != null) {
      this.json = this.jsonServerSideRendering;
      this.json.IsServerSideRendering = true;
    } else {
      this.json = jsonBrowser;
      this.json.IsServerSideRendering = false;
      if (window.location.href.startsWith("http://localhost:4200/")) { // Running in Framework\Framework.Angular\application\
        this.json.EmbeddedUrl = "http://localhost:50919/";
        this.update(<RequestJson> { Command: 0 });
      } 
      if (window != null) { // Running on client.
        this.json.RequestUrl = window.location.href;
        this.update(<RequestJson> { Command: 0 });
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

  public update(requestJson: RequestJson): void {
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
        requestUrl = new URL("/app.json?isEmbeddedUrl", this.json.EmbeddedUrl).href 
      } else {
        requestUrl = new URL("/app.json", this.json.RequestUrl).href 
      }

      let json: any = this.json;
      if (requestJson != null) {
         requestJson.IsRequestJson = true;
         requestJson.RequestCount = this.json.RequestCount;
         json = requestJson;
      }

      this.httpClient.request("POST", requestUrl, {
        body: JSON.stringify(json),
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
          this.update(<RequestJson> { Command: 0 }); // Process new merged request.
        }
        this.json.IsServerSideRendering = false;
        if (this.json.IsReload) {
          location.reload(true);
        }
      }, error => {
        this.isRequestPending = false;
      });
    }
  }
}
