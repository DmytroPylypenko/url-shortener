export interface CreateShortUrlRequest {
  originalUrl: string;
}

export interface CreateShortUrlResponse {
  id: number;
  originalUrl: string;
  shortCode: string;
}
