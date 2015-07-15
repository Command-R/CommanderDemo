import {EventAggregator} from 'aurelia-event-aggregator';
import {ContactUpdated} from './messages';

export class Notifications {
    static inject = [EventAggregator];
    constructor(ea) {
        $(function () {
            var hub = $.connection.notificationHub;
            if (!hub)
                return;

            hub.client.publish = function(message) {
                setTimeout(function() {
                    alert(message.Message);
                    ea.publish(new ContactUpdated(null));
                }, 1);
            }

            $.connection.hub.logging = true;
            $.connection.hub.url = "/signalr";
            $.connection.hub.start().done(function(resp) {
                console.log("SignalR Connected");
                console.log(resp);
            }).fail(function(err) {
                console.log("SignalR Connect ERROR");
                console.log(err);
            });
        });
    }
}
