import {EventAggregator} from 'aurelia-event-aggregator';
import {WebAPI} from './web-api';
import {ContactUpdated, ContactViewed} from './messages';
import {Router} from 'aurelia-router';

export class ContactList {
    static inject = [WebAPI, EventAggregator, Router];
    constructor(api, ea, router){
        this.api = api;
        this.contacts = [];
        this.router = router;

        ea.subscribe(ContactViewed, msg => this.select(msg.contact));
        ea.subscribe(ContactUpdated, msg => {
            this.api.getContactList().then(contacts => {
                this.contacts = contacts;
                if (!msg.contact) {
                    this.router.navigateToRoute('home');
                } else if (msg.contact.id !== this.selectedId) {
                    this.router.navigateToRoute('contacts', {id:msg.contact.id});
                }
            }).catch(function(){});
        });
    }

    created(){
        this.api.getContactList().then(contacts => {
            this.contacts = contacts;
        }).catch(function(){});
    }

    select(contact){
        this.selectedId = contact.id;
        return true;
    }
}
