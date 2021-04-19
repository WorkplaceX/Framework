import { DOCUMENT, Location } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Inject, Injectable, Renderer2, RendererFactory2 } from '@angular/core';
import { Title } from '@angular/platform-browser';

// See also https://angular.io/guide/http
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class Json { // AppJson
  Id!: number;

  Name: string | undefined;
  
  Type: string | undefined;
  
  CssClass: string | undefined;

  Version: string | undefined;

  VersionBuild: string | undefined;

  IsServerSideRendering: boolean | undefined;

  Session: string | undefined;

  SessionApp: string | undefined;

  SessionState: string | undefined;

  RequestCount: number | undefined;

  ResponseCount: number | undefined;

  RequestUrlHost: string | undefined;

  EmbeddedUrl: string | undefined;

  IsReload: boolean | undefined;

  List: any;

  DownloadData: string | undefined;

  DownloadFileName: string | undefined;

  DownloadContentType: string | undefined;

  IsScrollToTop: boolean | undefined;

  NavigatePathAddHistory: string | undefined;

  Title: string | undefined;

  CellList: Cell[] | undefined;

  StyleColumnList: StyleColumn[] | undefined;

  IsShowSpinner: boolean | undefined;

  IsHidePagination: boolean | undefined;

  IsShowConfig: boolean | undefined;

  IsShowConfigDeveloper: boolean | undefined;

  StyleRowList: StyleRow[] | undefined;

  TextHtml: string | undefined;

  IsNoSanatize: boolean | undefined;
  
  IsNoSanatizeScript: string | undefined;

  Key: string | undefined; // BingMap

  BrandTextHtml: string | undefined; // BootstrapNavbar, BulmaNavbar

  ItemStartList: any;

  ItemEndList: any;

  ButtonList: any;
}

export class CommandJson {
  CommandEnum: number | undefined;

  GridCellId: number | undefined;

  GridCellText: string | undefined;

  GridCellTextBase64: string | undefined;

  GridCellTextBase64FileName: string | undefined;

  ComponentId: number | undefined;

  GridIsClickEnum : number | undefined;

  BootstrapNavbarButtonId: number | undefined;
  
  BulmaNavbarItemId: number | undefined;

  BulmaFilterText: string | undefined;

  NavigatePath: string | undefined;

  HtmlButtonId: string | undefined;

  ResizeColumnIndex: number | undefined;

  ResizeColumnWidthValue: number | undefined;
}

export class RequestJson {
  RequestCount: number | undefined;

  ResponseCount: number | undefined;

  BrowserNavigatePathPost: string | undefined;

  CommandList: CommandJson[] | undefined;
}

export class Cell {
  CellEnum!: number;

  Width: string | undefined;

  IsSort: boolean | undefined;

  IsShowSpinner: boolean | undefined;

  Description: string | undefined;

  ColumnText: string | undefined;

  IsSelect: boolean | undefined;

  Html: string | undefined;

  HtmlLeft: string | undefined;

  HtmlRight: string | undefined;

  IsPassword: boolean | undefined;

  HtmlIsEdit: boolean | undefined;
  
  Align: number | undefined; // CellAnnotationAlignEnum
  
  Text: string | undefined;

  IsReadOnly: boolean | undefined;

  IsMultiline: boolean | undefined;

  Placeholder: string | undefined;

  ErrorParse: string | undefined;

  ErrorSave: string | undefined;

  Warning: string | undefined;

  IsFileUpload: boolean | undefined;

  GridLookup: any;
}

export class StyleRow {
  
}

export class StyleColumn {
  Width: string | undefined;

  WidthValue: number | undefined;

  WidthUnit: string | undefined;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {

  public json: Json = new Json();

  public VersionBuild: string = "Build (WorkplaceX=v3.49.00; Commit=a0d1b52; Pc=DEVPC; Time=2021-03-02 14:34:12 (UTC);)"; /* VersionBuild */

  public isRequestPending: boolean = false; // Request is in prgress.

  requestJsonQueue: RequestJson | undefined // Put latest request in queue, if waiting for pending request.

  public renderer: Renderer2; // Used for BingMap

  constructor(
    private httpClient: HttpClient, // app.module.ts imports: [HttpClientModule
    @Inject('jsonServerSideRendering') private jsonServerSideRendering: any, 
    @Inject(DOCUMENT) public document: Document, 
    rendererFactory: RendererFactory2, 
    private location: Location,
    private titleService: Title)  
  { 
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
        this.json = jsonBrowser; // Provided by index.html coming from ASP.NET Core WebServer for both SSR and not SSR.
      }

      this.json.IsServerSideRendering = false;
      if (window.location.href.endsWith(":4200/")) { // Running in Framework/Framework.Angular/application/
        this.json.EmbeddedUrl = "http://" + window.location.hostname + ":5000/"; // http://localhost:5000/ or http://localhost2:5000/
      } 
      if (window != null) { // Running on client.
        this.json.RequestUrlHost = window.location.href;
        if (typeof jsonBrowser == 'undefined')
        {
          this.update(null); // First request if running on http://localhost:4200
        }
      }
    }
  }

  private fileDownload(jsonResponse: Json) { // See also: https://stackoverflow.com/questions/16245767/creating-a-blob-from-a-base64-string-in-javascript
    if (jsonResponse.DownloadFileName != null) {
      const byteCharacters = atob(jsonResponse.DownloadData || "");
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

  public update(commandJson: CommandJson | null): void {
    var requestJson = <RequestJson> <unknown>{ CommandList: [] }
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
      let requestUrlHost;
      if (this.json.EmbeddedUrl != null) {
        requestUrlHost = new URL("/app.json", this.json.EmbeddedUrl).href 
      } else {
        requestUrlHost = new URL("/app.json", this.json.RequestUrlHost).href 
      }

      requestJson.RequestCount = this.json.RequestCount;
      requestJson.ResponseCount = this.json.ResponseCount;
      requestJson.BrowserNavigatePathPost = window.location.href;

      // app.json POST
      this.httpClient.post<Json>(
        requestUrlHost, 
        JSON.stringify(requestJson),
        { withCredentials: true }
      )
      .pipe(catchError((error: HttpErrorResponse) => { // import { catchError } from 'rxjs/operators';
        this.isRequestPending = false;
        return throwError("app.json POST failed!"); // import { throwError } from 'rxjs';
      }))
      .subscribe((data: Json) => {
        let jsonResponse = data;
        this.fileDownload(jsonResponse);
        this.isScrollToTop(jsonResponse);
        if (this.requestJsonQueue == null) { // Apply response if there is no newer request.
          // Update UI
          this.json = jsonResponse;
          this.isRequestPending = false;
          // Set application title
          if (this.json.Title != null) {
            this.titleService.setTitle(this.json.Title);
          }
        } else {
          this.json.ResponseCount = jsonResponse.ResponseCount;
          let requestJsonQueue = this.requestJsonQueue;
          this.requestJsonQueue =  undefined;
          this.isRequestPending = false;
          if (requestJsonQueue.CommandList != null) {
            this.update(requestJsonQueue.CommandList[0]); // Process latest request.
          }
        }
        this.json.IsServerSideRendering = false;
        if (this.json.IsReload) {
          setTimeout(() => {
            window.location.reload();
          }, 1000); // Wait one second then reload.
        }
        if (this.json.NavigatePathAddHistory != null) {
          this.location.go(this.json.NavigatePathAddHistory, "", this.json.NavigatePathAddHistory); // Put path into state because Angular does not handle traling slash in location. // import { Location } from '@angular/common';
        }
      });
    } else {
      this.requestJsonQueue = requestJson;
    }
  }
}
