import { Routes } from '@angular/router';
import { SearchRoot } from './search/search-root/search-root';
import { StatisticsRoot } from './statistics/statistics-root/statistics-root';

export const routes: Routes = [
  {
    path: 'search',
    component: SearchRoot,
    title: 'JR Accordion | Search',
  },
  {
    path: 'statistics',
    component: StatisticsRoot,
    title: 'JR Accordion | Statistics',
  },
  {
    path: '**',
    redirectTo: 'search',
  },
];