import { inject, Injectable } from '@angular/core';
import { Config } from './config';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private http = inject(HttpClient);
  private configSubject = new BehaviorSubject<Config | null>(null);

  initialize(): Observable<unknown> {
    return this.http.get<Config>('/config.json')
      .pipe(tap(config => this.configSubject.next(config)));
  }

  config$ = this.configSubject.asObservable();
  get config(): Config | null {
    return this.configSubject.getValue();
  }
}