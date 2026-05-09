import { HttpClient } from '@angular/common/http';
import { Component, signal, OnInit } from '@angular/core';

interface NewsAlert {
  id: number;
  sourceId: string;
  headline: string;
  urgencyLevel: string;
  detailedInstructions: string;
  systemTimestamp: string;
}


@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})



export class App implements OnInit {
  public newsHeadlines: NewsAlert[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getNews();
  }

  getNews() {
    this.http.get<NewsAlert[]>('/api/news/headlines').subscribe({
      next: (result) => {
        this.newsHeadlines = result;
      },
      error: (error) => {
        console.error("Could not load the news room:", error);
      }
    });
  }

  protected readonly title = signal('arkwatch.client');
}
