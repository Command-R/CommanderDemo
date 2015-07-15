import {HttpClient} from 'aurelia-http-client';

class Request {
    constructor(method, params) {
        this.jsonrpc = "2.0";
        this.id = guid();
        this.method = method;
        this.params = params || {};
    }
}

class Response {
    constructor(response) {
        this.jsonrpc = response.jsonrpc;
        this.id = response.id;
        this.result = response.result;
        this.error = response.error;
    }
}

function guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

//NOTE: this is an incomplete implementation of JsonRpc for Aurelia just for the demo.
export class JsonRpc {
    static inject = [HttpClient];
    constructor(http) {
        this.http = http;
    }

    send(method, params) {
        return new Promise((resolve, reject) => {
            return this.http
            .createRequest('/jsonrpc')
            .asPost()
            .withHeader('Authorization', window.JsonRpcToken)
            .withContent(new Request(method, params))
            .send()
            .then(response => {
                var resp = new Response(response.content);
                if (resp.error) {
                    reject(resp.error);
                }else {
                    resolve(resp.result);
                }
            }).catch(error => {
                reject(error);
            });
        });
    }
}
