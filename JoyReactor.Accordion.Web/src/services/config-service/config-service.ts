import { inject, Injectable } from '@angular/core';
import { Config } from './config';
import { BehaviorSubject, catchError, Observable, of, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  private http = inject(HttpClient);
  private configSubject = new BehaviorSubject<Config | null>(null);

  initialize(): Observable<unknown> {
    const url = environment.production ? '/config.json' : '/config.development.json';
    return this.http.get<Config>(url)
      .pipe(
        catchError(err => {
          console.error('Failed to load config.', err);
          return of(null);
        }),
        tap(config => this.configSubject.next(config)));
  }

  config$ = this.configSubject.asObservable();
  get config(): Config | null {
    return this.configSubject.getValue();
  }
}