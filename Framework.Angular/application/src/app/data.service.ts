import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class RequestJson {
  Command: number;

  Grid2CellId: number;

  Grid2CellText: string;

  Grid2StyleColumnList: string[];
  
  ComponentId: number;

  GridColumnId: number;

  GridRowId: number;

  GridCellId: number;

  GridCellText: string;

  GridIsClickEnum : number;

  BootstrapNavbarButtonId: number;

  RequestCount: number;

  ResponseCount: number;

  BrowserUrl: string;

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

  ResponseCount: number;

  RequestUrl: string;

  EmbeddedUrl: string;

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

  constructor(private httpClient: HttpClient, @Inject('jsonServerSideRendering') private jsonServerSideRendering: any) { 
    if (this.jsonServerSideRendering != null) {
      this.json = this.jsonServerSideRendering;
      this.json.IsServerSideRendering = true;
    } else {
      this.json = jsonBrowser;
      this.json.IsServerSideRendering = false;
      if (window.location.href.startsWith("http://localhost:4200/")) { // Running in Framework\Framework.Angular\application\
        this.json.EmbeddedUrl = "http://localhost:50919/";
      } 
      if (window != null) { // Running on client.
        this.json.RequestUrl = window.location.href;
        this.update(<RequestJson> { Command: 0 });
      }
    }
  }

  requestJsonQueue: RequestJson // Put latest request in queue, if waiting for pending request.

  public update(requestJson: RequestJson): void {
    if (this.isRequestPending == false) { // Do not send a new request while old is still processing.
      // RequestCount
      if (this.json.RequestCount == null) {
        this.json.RequestCount = 0;
      }
      this.json.RequestCount += 1;
      this.isRequestPending = true;
      let requestUrl;
      if (this.json.EmbeddedUrl != null) {
        requestUrl = new URL("/app.json", this.json.EmbeddedUrl).href 
      } else {
        requestUrl = new URL("/app.json", this.json.RequestUrl).href 
      }

      requestJson.RequestCount = this.json.RequestCount;
      requestJson.ResponseCount = this.json.ResponseCount;
      requestJson.BrowserUrl = window.location.href;

      this.httpClient.request("POST", requestUrl, {
        body: JSON.stringify(requestJson),
        withCredentials: true,
      })
      .subscribe(body => {
        let jsonResponse = <Json>body;
        if (this.requestJsonQueue == null) { // Apply response if there is no newer request.
          this.json = jsonResponse;
          this.isRequestPending = false;
        } else {
          this.json.ResponseCount = jsonResponse.ResponseCount;
          let requestJsonQueue = this.requestJsonQueue;
          this.requestJsonQueue = null;
          this.isRequestPending = false;
          this.update(requestJsonQueue); // Process latest request.
        }
        this.json.IsServerSideRendering = false;
        if (this.json.IsReload) {
          location.reload(true);
        }
      }, error => {
        this.isRequestPending = false;
      });
    } else {
      this.requestJsonQueue = requestJson;
    }
  }
}
