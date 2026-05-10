import { HttpClient } from '@angular/common/http';
import { Component, OnInit, ChangeDetectorRef, AfterViewInit, OnDestroy } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App implements OnInit, AfterViewInit, OnDestroy {
  public allHeadlines: any[] = [];
  public newsHeadlines: any[] = [];
  public tickerItems: any[] = []; 
  public currentIndex = 0;
  public isLocalMode = false;
  public selectedAlert: any = null;

  private map!: L.Map;
  private cycleInterval: any;
  private readonly localKeyword = "CLEBURNE";

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.fetchData();
    setInterval(() => this.fetchData(), 30000);
    this.startAlertCycle();
  }

  ngAfterViewInit() {
    this.initMap();
  }

  ngOnDestroy() {
    if (this.cycleInterval) clearInterval(this.cycleInterval);
  }

  private fetchData() {
    this.http.get<any[]>('/api/news/headlines').subscribe({
      next: (data) => {
        this.allHeadlines = data || [];
        this.applyFilters();
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Fetch Error:", err)
    });
  }

  private startAlertCycle() {
    if (this.cycleInterval) clearInterval(this.cycleInterval);
    this.cycleInterval = setInterval(() => this.nextAlert(), 8000);
  }

  public nextAlert() {
    if (this.tickerItems.length > 0) {
      this.currentIndex = (this.currentIndex + 1) % this.tickerItems.length;
      this.cdr.detectChanges();
    }
  }

  public prevAlert() {
    if (this.tickerItems.length > 0) {
      this.currentIndex = (this.currentIndex - 1 + this.tickerItems.length) % this.tickerItems.length;
      this.cdr.detectChanges();
    }
  }

  public toggleViewMode() {
    this.isLocalMode = !this.isLocalMode;
    this.currentIndex = 0; 
    this.applyFilters();
  }

  private applyFilters() {
    if (this.isLocalMode) {
      this.newsHeadlines = this.allHeadlines.filter(h =>
        (h.Headline || h.headline)?.toUpperCase().includes(this.localKeyword)
      );

      this.tickerItems = [...this.newsHeadlines];

      if (this.tickerItems.length === 0) {
        this.tickerItems = [{
          Headline: "NO ACTIVE LOCAL THREATS: CLEBURNE COUNTY",
          UrgencyLevel: "info",
          Expiration: "N/A"
        }];
      }
    } else {
      this.newsHeadlines = [...this.allHeadlines];
      this.tickerItems = [...this.allHeadlines];

      if (this.tickerItems.length === 0) {
        this.tickerItems = [{
          Headline: "ARKWATCH: NO ACTIVE STATEWIDE THREATS",
          UrgencyLevel: "info",
          Expiration: "N/A"
        }];
      }
    }
    this.currentIndex = 0;
    this.cdr.detectChanges();
  }

  public openModalFromTicker() {
    if (this.tickerItems.length > 0) {
      this.openModal(this.tickerItems[this.currentIndex]);
    }
  }

  private initMap(): void {
    setTimeout(() => {
      const container = document.getElementById('map');
      if (!container) return;

      this.map = L.map('map', { scrollWheelZoom: false }).setView([34.7465, -92.2896], 7);
      L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}').addTo(this.map);

      L.marker([35.4806, -91.9904]).addTo(this.map)
        .bindPopup('ArkWatch HQ: Heber Springs');

      this.map.invalidateSize();
    }, 200);
  }

  openModal(alert: any) {
    if (alert && (alert.headline || alert.Headline)) {
      this.selectedAlert = alert;
    }
  }

  closeModal() { this.selectedAlert = null; }
}
