import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

declare var jsonBrowser: any; // Data from browser, sent by server on first request.

export class Json {
  Name: string;

  Version: string;

  VersionBuild: string;

  IsServerSideRendering: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public Json: Json;

  constructor(httpClient: HttpClient, @Inject('jsonServerSideRendering') private jsonServerSideRendering: any) { 
    if (jsonServerSideRendering != null) {
      this.Json = jsonServerSideRendering;
    } else {
      this.Json = jsonBrowser;
    }
  }
}
