import { Inject } from '@angular/core';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import * as util from './util';
import {Observable} from 'rxjs/Rx';
import 'rxjs/add/operator/map';

declare var browserJson: any; // Params from browser

export class Json {
    Name: string;
    RequestUrl: string;
    VersionClient: string; // Angular client version.
    VersionServer: string; // Angular client version.
    List:any;
    IsBrowser:any;
    Session:string;
    RequestLog: string;
    RequestCount: number;
    ResponseCount: number;
    GridDataJson: any;
    ErrorProcess: string;
}

@Injectable()
export class DataService {

    json: Json;

    log: string;

    RequestCount: number;

    http: Http;

    constructor( @Inject('angularJson') angularJson: string, @Inject('requestBodyJson') private requestBodyJson: any, http: Http) {
        this.http = http;
        // Request json coming from web post to universal. // Universal mode.
        if (requestBodyJson != null)
        {
            this.json = requestBodyJson;
        }
        else
        {
            // Angular json coming from client app.module.ts // Client debug mode.
            if (angularJson != null) {
                this.json = JSON.parse(angularJson);
                this.json.RequestUrl = "http://localhost:49323/"; // Server running in Visual Studio.
                this.update();
            }
            else
            {
                // Normal mode.
                this.json = browserJson;
                this.log = "";
                this.json.Name = "dataService.ts=" + util.currentTime();
                this.json.RequestUrl = "";
            }
        }
    }

    update() {
        if (this.RequestCount == null) { this.RequestCount = 0; };
        if (this.log == null) { this.log = "" };
        this.RequestCount += 1;
        this.json.RequestCount = this.RequestCount;
        // POST
        this.log += "Send POST; ";
        this.http.post(this.json.RequestUrl + 'Application.json', JSON.stringify(this.json))
        .map(res => res)
        .subscribe(
            body => { 
                var jsonReceive: Json = <Json>(body.json());
                if (this.json.RequestCount == jsonReceive.RequestCount) {
                    this.json = jsonReceive;
                }
            },
            err => this.log += err + "; ",
            () => this.log += "Receive; "
        );
    }
}