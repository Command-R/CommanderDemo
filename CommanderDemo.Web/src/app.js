import {WebAPI} from './web-api';
import {Notifications} from './notifications';

export class App {
    static inject = [WebAPI, Notifications];
    constructor(api, notifications) {
        this.api = api;
        this.notifications = notifications;
    }

    configureRouter(config, router){
        config.title = 'Contacts';
        config.map([
            { route: '',              moduleId: 'no-selection',   name:'home',    title: 'Select'},
            { route: 'contacts/:id',  moduleId: 'contact-detail', name:'contacts'                }
        ]);

        this.router = router;
    }
}
