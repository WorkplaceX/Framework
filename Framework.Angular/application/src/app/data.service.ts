import { Injectable } from '@angular/core';
import { Inject, RendererFactory2, Renderer2 } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { DOCUMENT, Location } from '@angular/common';
import { Title } from '@angular/platform-browser';

// See also https://angular.io/guide/http
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class Json { // AppJson
  Id: number;

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

  PathAddHistory: string;

  Title: string;
}

export class CommandJson {
  Command: number;

  GridCellId: number;

  GridCellText: string;

  GridCellTextBase64: string;

  GridCellTextBase64FileName: string;

  GridStyleColumnList: string[];
  
  ComponentId: number;

  GridIsClickEnum : number;

  BootstrapNavbarButtonId: number;
  
  BulmaNavbarItemId: number;

  BulmaFilterText: string;

  NavigateLinkPath: string;
}

export class RequestJson {
  RequestCount: number;

  ResponseCount: number;

  BrowserUrl: string;

  CommandList: CommandJson[];
}

@Injectable({
  providedIn: 'root'
})
export class DataService {

  public json: Json = new Json();

  public VersionBuild: string = "Build (local)"; /* VersionBuild */

  public isRequestPending: boolean = false; // Request is in prgress.

  requestJsonQueue: RequestJson // Put latest request in queue, if waiting for pending request.

  public renderer: Renderer2; // Used for BingMap

  constructor(
      private httpClient: HttpClient, 
      @Inject('jsonServerSideRendering') private jsonServerSideRendering: any, 
      @Inject(DOCUMENT) public document: Document, 
      rendererFactory: RendererFactory2, 
      private location: Location,
      private titleService: Title) { 
    this.renderer = rendererFactory.createRenderer(null, null);
    if (this.jsonServerSideRendering != null) {
      // Render on SSR server
      this.json = this.jsonServerSideRendering;
      this.json.IsServerSideRendering = true;
      if (this.json.Title != null) {
        this.titleService.setTitle(this.json.Title);
      }
    } else {
      // Render in browser
      if (typeof jsonBrowser == 'undefined') { // jsonBrowser not declared in index.html
        this.json = <Json>{};
      } else
      {
        this.json = jsonBrowser; // Provided by SSR in index.html see also method WebsiteServerSideRenderingAsync();
      }

      this.json.IsServerSideRendering = false;
      if (window.location.href.startsWith("http://localhost:4200/")) { // Running in Framework\Framework.Angular\application\
        this.json.EmbeddedUrl = "http://localhost:50919/";
      } 
      if (window != null) { // Running on client.
        this.json.RequestUrl = window.location.href;
        this.update(null);
      }
    }
  }

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

  public update(commandJson: CommandJson): void {
    var requestJson = <RequestJson> { CommandList: [] }
    if (commandJson != null) {
      requestJson.CommandList = [ commandJson ];
    }

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

      // app.json POST
      this.httpClient.post<Json>(
        requestUrl, 
        JSON.stringify(requestJson),
        { withCredentials: true }
      )
      .pipe(catchError((error: HttpErrorResponse) => {
        this.isRequestPending = false;
        return throwError("app.json POST failed!");
      }))
      .subscribe((data: Json) => {
        let jsonResponse = data;
        this.fileDownload(jsonResponse);
        this.isScrollToTop(jsonResponse);
        if (this.requestJsonQueue == null) { // Apply response if there is no newer request.
          this.json = jsonResponse;
          this.isRequestPending = false;
          if (this.json.Title != null) {
            this.titleService.setTitle(this.json.Title);
          }
        } else {
          this.json.ResponseCount = jsonResponse.ResponseCount;
          let requestJsonQueue = this.requestJsonQueue;
          this.requestJsonQueue = null;
          this.isRequestPending = false;
          this.update(requestJsonQueue.CommandList[0]); // Process latest request.
        }
        this.json.IsServerSideRendering = false;
        if (this.json.IsReload) {
          setTimeout(() => {
            window.location.reload();
          }, 1000); // Wait one second then reload.
        }
        if (this.json.PathAddHistory != null) {
          this.location.go(this.json.PathAddHistory, "", this.json.PathAddHistory); // Put path into state because Angular does not handle traling slash in location.
        }
      });
    } else {
      this.requestJsonQueue = requestJson;
    }
  }
}