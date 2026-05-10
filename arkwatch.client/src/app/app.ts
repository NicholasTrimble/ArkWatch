import { HttpClient } from '@angular/common/http';
import { Component, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App implements OnInit, AfterViewInit {
  private allHeadlines: any[] = [];
  private allTickerItems: any[] = [];
  private map!: L.Map;

  public newsHeadlines: any[] = [];
  public tickerItems: any[] = [];
  public isLocalMode: boolean = false;
  public selectedAlert: any = null;
  private readonly localKeyword = "CLEBURNE";

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.fetchData();
    setInterval(() => this.fetchData(), 30000);
  }

  ngAfterViewInit() {
    this.initMap();
  }

  private fetchData() {
    this.getNews();
    this.getTicker();
  }

  // FIXED: Added back the missing toggle method
  public toggleViewMode() {
    this.isLocalMode = !this.isLocalMode;
    this.applyFilters();
  }

  private initMap(): void {
    this.map = L.map('map', {
      zoomControl: true,
      scrollWheelZoom: false,
      dragging: true,
    }).setView([34.7465, -92.2896], 7);

    L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}', {
      attribution: '&copy; OpenStreetMap contributors &copy; CARTO',
      subdomains: 'abcd',
      maxZoom: 20
    }).addTo(this.map);

    L.marker([35.4806, -91.9904]).addTo(this.map)
      .bindPopup('ArkWatch HQ: Cleburne County');
  }

  private applyFilters() {
    if (this.isLocalMode) {
      this.newsHeadlines = this.allHeadlines.filter(h =>
        h.headline.toUpperCase().includes(this.localKeyword)
      );
      this.tickerItems = this.allTickerItems.filter(t =>
        t.text && t.text.toUpperCase().includes(this.localKeyword)
      );

      if (this.tickerItems.length === 0) {
        this.tickerItems = [{ text: "NO ACTIVE LOCAL THREATS FOR CLEBURNE COUNTY", category: "info" }];
      }
    } else {
      this.newsHeadlines = [...this.allHeadlines];
      this.tickerItems = [...this.allTickerItems];
    }
    this.cdr.detectChanges();
  }

  getTicker() {
    this.http.get<any[]>('/api/news/ticker').subscribe({
      next: (result) => {
        this.allTickerItems = result;
        this.applyFilters();
      },
      error: (err) => console.error('Ticker Error:', err)
    });
  }

  getNews() {
    this.http.get<any[]>('/api/news/headlines').subscribe({
      next: (result) => {
        this.allHeadlines = result;
        this.applyFilters();
      },
      error: (err) => console.error('News Error:', err)
    });
  }

  openModal(alert: any) {
    this.selectedAlert = alert;
  }

  closeModal() {
    this.selectedAlert = null;
  }
}
