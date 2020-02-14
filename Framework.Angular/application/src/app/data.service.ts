import { Injectable, Inject, Renderer2, RendererFactory2 } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class RequestJson {
  Command: number;

  GridCellId: number;

  GridCellText: string;

  GridStyleColumnList: string[];
  
  ComponentId: number;

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

  DownloadData: string;

  DownloadFileName: string;

  DownloadContentType: string;

  IsScrollToTop: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public json: Json = new Json();
  
  public VersionBuild: string = "Build (local)"; /* VersionBuild */
  
  public isRequestPending: boolean = false; // Request is in prgress.

  public renderer: Renderer2;

  constructor(private httpClient: HttpClient, @Inject('jsonServerSideRendering') private jsonServerSideRendering: any, @Inject(DOCUMENT) public document: Document, private rendererFactory: RendererFactory2) { 
    this.renderer = rendererFactory.createRenderer(null, null);
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

  private fileDownload(jsonResponse: Json) { // See also: https://stackoverflow.com/questions/16245767/creating-a-blob-from-a-base64-string-in-javascript
    if (jsonResponse.DownloadFileName != null) {
      const byteCharacters = atob(jsonResponse.DownloadData);
      const byteNumbers = new Array(byteCharacters.length);
      for (let i = 0; i < byteCharacters.length; i++) {
          byteNumbers[i] = byteCharacters.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers);          
      const blob = new Blob([byteArray], {type: jsonResponse.DownloadContentType});

      if (window.navigator && window.navigator.msSaveOrOpenBlob) {
        window.navigator.msSaveOrOpenBlob(blob, jsonResponse.DownloadFileName); // See also: https://stackoverflow.com/questions/27463901/setting-window-location-or-window-open-in-angularjs-gives-access-is-denied-in
      }
      else {
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = jsonResponse.DownloadFileName;
        link.click();
        URL.revokeObjectURL(link.href);
      }      
    }
  }

  private isScrollToTop(jsonResponse: Json) {
    if (jsonResponse.IsScrollToTop) {
      document.documentElement.scrollTop = 0;
    }
  }

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
      .subscribe((data: Json) => {
        let jsonResponse = data;
        this.fileDownload(jsonResponse);
        this.isScrollToTop(jsonResponse);
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
          setTimeout(() => {
            location.reload(true);
          }, 1000); // Wait one second then reload.
        }
      }, error => {
        this.isRequestPending = false;
      });
    } else {
      this.requestJsonQueue = requestJson;
    }
  }
}
