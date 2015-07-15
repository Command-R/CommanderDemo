import {inject} from 'aurelia-framework';
import {JsonRpc} from 'json-rpc';

function map(contact) {
    return {
        id: contact.Id,
        firstName: contact.FirstName,
        lastName: contact.LastName,
        email: contact.Email,
        phoneNumber: contact.PhoneNumber
    }
}

@inject(JsonRpc)
export class WebAPI {
    jsonRpc;

    constructor(jsonRpc) {
        this.jsonRpc = jsonRpc;
    }

    getContactList() {
        this.isRequesting = true;
        return new Promise((resolve, reject) => {
            this.jsonRpc.send('QueryContacts').then(resp => {
                this.isRequesting = false;
                resolve(resp.Items.map(map));
            }).catch(error => {
                this.isRequesting = false;
                console.log("getContactList ERROR", error);
                alert(error.message);
                reject(error);
            });
        });
    }

    getContactDetails(id) {
        this.isRequesting = true;
        return new Promise((resolve, reject) => {
            this.jsonRpc.send('GetContact', {Id:id}).then(resp => {
                this.isRequesting = false;
                resolve(map(resp));
            }).catch(error => {
                this.isRequesting = false;
                console.log("getContactDetails ERROR", error);
                alert(error.message);
                reject(error);
            });
        });
    }

    saveContact(contact) {
        this.isRequesting = true;
        return new Promise((resolve, reject) => {
            this.jsonRpc.send('SaveContact', contact).then(id => {
                this.jsonRpc.send('GetContact', {Id:id}).then(resp => {
                    this.isRequesting = false;
                    resolve(map(resp));
                });
            }).catch(error => {
                this.isRequesting = false;
                console.log("saveContact ERROR", error);
                alert(error.message);
                reject(error);
            });
        });
    }

    deleteContact(contact) {
        this.isRequesting = true;
        return new Promise((resolve, reject) => {
            this.jsonRpc.send('DeleteContact', {Id:contact.id}).then(resp => {
                this.isRequesting = false;
                resolve(resp);
            }).catch(error => {
                this.isRequesting = false;
                console.log("deleteContact ERROR", error);
                alert(error.message);
                reject(error);
            });
        });
    }
}
