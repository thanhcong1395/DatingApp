import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnetion?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService) { }

  createHubConnection(user: User) {
    this.hubConnetion = new HubConnectionBuilder().withUrl(this.hubUrl + 'presence', { accessTokenFactory: () => user.token }).withAutomaticReconnect().build();

    this.hubConnetion.start().catch(error => console.log(error));

    this.hubConnetion.on('UserIsOnline', username => {
      this.toastr.info(username + ' has connected');
    })

    this.hubConnetion.on('UserIsOffline', username => {
      this.toastr.info(username + ' has disconnected');
    })

    this.hubConnetion.on('GetOnlineUsers', username => {
      this.onlineUsersSource.next(username);
    });
  }

  stopHubConnection() {
    this.hubConnetion?.stop().catch(error => console.log(error))
  }
}
