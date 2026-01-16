import { CommonModule } from '@angular/common';
import { Component, EventEmitter, inject, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { isIP } from 'is-ip';
import { SearchService } from '../search-service/search-service';

@Component({
  selector: 'app-search-picture',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-picture.html',
  styleUrl: './search-picture.scss',
})
export class SearchPicture {
  private searchService = inject(SearchService);
  @Output() onFileSelected = new EventEmitter<File>();
  allowedTypes: string[] = ['image/png', 'image/jpeg', 'image/gif', 'image/bmp', 'image/tiff', 'video/mp4', 'video/webm'];
  isDragging: boolean = false;
  url: string = "";

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.onFileSelected.emit(input.files[0]);
    }
  }

  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      if (file.type.startsWith('image/')) {
        this.onFileSelected.emit(file);
      }
    }
  }

  onFileDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onFileDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onUrlChange(event: Event): void {
    this.url = decodeURIComponent(this.url);
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

  search(): void {
    this.searchService.searchMediaByUrl(this.url)
      .subscribe(v => console.log(v), e => console.error(e));
  }
}