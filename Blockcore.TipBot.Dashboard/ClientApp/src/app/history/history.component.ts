import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
// import {  } from '@fortawesome/free-solid-svg-icons';
import { faDiscord } from '@fortawesome/free-brands-svg-icons';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html'
})
export class HistoryComponent {
  public forecasts: any[];

  faDiscord = faDiscord;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<any[]>(baseUrl + 'weatherforecast/history').subscribe(result => {
      this.forecasts = result;
    }, error => console.error(error));
  }
}
