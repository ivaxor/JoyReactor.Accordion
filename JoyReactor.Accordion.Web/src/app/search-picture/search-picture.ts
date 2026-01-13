import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { isIP } from 'is-ip';
import { SearchService } from '../../services/search-service/search-service';
@Component({
  selector: 'app-search-picture',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-picture.html',
  styleUrl: './search-picture.scss',
})
export class SearchPicture {
  private searchService = inject(SearchService);
  url: string = "";

  onModelChange(url: string): void {
    this.url = decodeURIComponent(url);
  }

  search(): void {
    this.searchService.searchMediaByUrl(this.url)
      .subscribe(v => console.log(v), e => console.error(e));
  }

  isUrlValid(): boolean {
    try {
      const url = new URL(this.url);

      if (url.protocol !== 'https:')
        return false;

      if (!url.host.includes('.'))
        return false;

      if (isIP(url.host))
        return false;

      return true;
    } catch {
      return false;
    }
  }
}