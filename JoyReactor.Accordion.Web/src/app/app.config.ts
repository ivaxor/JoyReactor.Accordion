import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import { ConfigService } from '../services/config-service/config-service';

export const appConfig: ApplicationConfig = {
  providers: [    
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideAppInitializer((configService = inject(ConfigService)) => configService.initialize()),
  ]
};