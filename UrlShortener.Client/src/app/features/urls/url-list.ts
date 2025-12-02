import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { UrlRecordListItem } from '../../models/url-record';
import { UrlsService } from '../../core/services/urls-service';
import { CreateShortUrlRequest } from '../../models/create-short-url';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-url-list',
  imports: [FormsModule, DatePipe],
  templateUrl: './url-list.html',
  styleUrl: './url-list.scss',
})
export class UrlListComponent {
  private readonly urlsService = inject(UrlsService);

  urls: UrlRecordListItem[] = [];
  newUrl: string = '';
  errorMessage?: string;

  ngOnInit(): void {
    this.loadUrls();
  }

  /** Loads all URL records from API */
  loadUrls() {
    this.urlsService.getAll().subscribe({
      next: (data) => (this.urls = data),
      error: () => (this.errorMessage = 'Failed to load URLs.'),
    });
  }

  /** Creates a new shortened URL */
  createUrl() {
    if (!this.newUrl.trim()) return;

    const request: CreateShortUrlRequest = {
      originalUrl: this.newUrl,
    };

    this.urlsService.create(request).subscribe({
      next: (created) => {
        // Add to table immediately (no reload)
        this.urls.unshift({
          id: created.id,
          originalUrl: created.originalUrl,
          shortCode: created.shortCode,
          createdBy: 'You',
          createdAtUtc: new Date().toISOString(),
          visitCount: 0,
        });

        this.newUrl = '';
        this.errorMessage = undefined;
      },
      error: (err) => {
        if (err.status === 409 || err.status === 400) {
          this.errorMessage = 'This URL already exists.';
        } else {
          this.errorMessage = 'Error creating URL.';
        }
      },
    });
  }

  /** Deletes a URL (only if allowed by backend) */
  deleteUrl(id: number) {
    this.urlsService.delete(id).subscribe({
      next: () => {
        this.urls = this.urls.filter((u) => u.id !== id);
      },
      error: () => {
        this.errorMessage = 'You are not allowed to delete this item.';
      },
    });
  }
}
