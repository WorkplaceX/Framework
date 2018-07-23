import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export class Json {
  Name: string;
}

@Injectable({
  providedIn: 'root'
})
export class DataService {
  public Json: Json;

  constructor(httpClient: HttpClient) { 
    this.Json = new Json();
    this.Json.Name = 'Init';
  }
}
