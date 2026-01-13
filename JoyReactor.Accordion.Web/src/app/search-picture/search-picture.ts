import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, Injectable } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { isIP } from 'is-ip';
import { SearchDownloadRequest } from '../search-download-request';
import { SearchResponse } from '../search-response';

@Component({
  selector: 'app-search-picture',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-picture.html',
  styleUrl: './search-picture.scss',
})
export class SearchPicture {
  @Injectable({ providedIn: 'root' })
  private http = inject(HttpClient);

  url: string = "";

  onModelChange(url: string): void {
    this.url = decodeURIComponent(url);
  }

  search(): void {
    const request: SearchDownloadRequest = {
      pictureUrl: this.url,
    };

    this.http.post<SearchResponse[]>("http://127.0.0.1:5288/search/pictures/download", request)
      .subscribe(response => console.log(response), err => console.error(err));
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