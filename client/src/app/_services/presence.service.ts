import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnetion?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnetion = new HubConnectionBuilder().withUrl(this.hubUrl + 'presence', { accessTokenFactory: () => user.token }).withAutomaticReconnect().build();

    this.hubConnetion.start().catch(error => console.log(error));

    this.hubConnetion.on('UserIsOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => this.onlineUsersSource.next([...usernames, username])
      })
    })

    this.hubConnetion.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => this.onlineUsersSource.next(usernames.filter(x => x !== username))
      })
    })

    this.hubConnetion.on('GetOnlineUsers', username => {
      this.onlineUsersSource.next(username);
    });

    this.hubConnetion.on('NewMessageReceived', ({ username, knowsAs }) => {
      this.toastr.info(knowsAs + ' has sent you a new message! Click me to see it').onTap.pipe(take(1)).subscribe({
        next: () => this.router.navigateByUrl('/members/' + username + '?tab=Messages')
      })
    })
  }

  stopHubConnection() {
    this.hubConnetion?.stop().catch(error => console.log(error))
  }
}
