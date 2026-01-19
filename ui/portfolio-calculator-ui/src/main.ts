import { bootstrapApplication } from '@angular/platform-browser';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { AppComponent } from './app/app';
import { appConfig } from './app/app.config';

bootstrapApplication(AppComponent, {
  providers: [
    ...appConfig.providers!,
    provideCharts(withDefaultRegisterables()),
  ],
}).catch(err => console.error(err));
