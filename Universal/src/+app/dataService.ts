import { Inject } from '@angular/core';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import * as util from './util';
import {Observable} from 'rxjs/Rx';
import 'rxjs/add/operator/map';

declare var browserJson: any; // Params from browser

export class Json {
    Name: string;
    IsJsonGet: boolean; // GET not POST json when debugging client. See also file Application.json
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

    constructor( @Inject('angularJson') angularJson: string, http: Http) {
        this.http = http;
        // Default data
        this.json = new Json();
        this.log = "";
        this.RequestCount = 0;
        this.json.Name = "dataService.ts=" + util.currentTime();
        // Angular universal json
        if (angularJson != null) {
            this.json = JSON.parse(angularJson);
        }
        // Browser json
        if (typeof browserJson !== 'undefined') {
            if (browserJson instanceof Object){
                this.json = browserJson;
            } else {
                this.json = JSON.parse(browserJson);
            }
        }
        //
        this.json.VersionClient = util.versionClient();
        //
        if (this.json.IsJsonGet == true) {
            this.update(); // For debug mode.
        }
        if (this.json.IsBrowser == true) {
            this.update();
        }
    }

    update() {
        this.RequestCount += 1;
        this.json.RequestCount = this.RequestCount;
        if (this.json.IsJsonGet == true) {
            // GET for debug
            this.log += "Send GET; "
            this.http.get('Application.json')
            .map(res => res)
            .subscribe(
                body => this.json = <Json>(body.json()),
                err => this.log += err + "; ",
                () => this.log += "Receive; "
            );
        } else {
            // POST
            this.log += "Send POST; ";
            this.http.post('Application.json', JSON.stringify(this.json))
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
}