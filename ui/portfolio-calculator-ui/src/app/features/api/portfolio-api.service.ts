import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export type InvestmentType = string;

export interface PortfolioTypeCompositionItemDto {
  type: InvestmentType;
  value: number;
}

export interface PortfolioValuationResultDto {
  totalValue: number;
  compositionByType: PortfolioTypeCompositionItemDto[];
}

@Injectable({ providedIn: 'root' })
export class PortfolioApiService {
  private readonly baseUrl = '/api/portfolio';

  constructor(private http: HttpClient) {}

  getPortfolioValue(investorId: string, date: string): Observable<PortfolioValuationResultDto> {
    const params = new HttpParams()
      .set('investorId', investorId)
      .set('date', date);

    return this.http.get<PortfolioValuationResultDto>(`${this.baseUrl}/value`, { params });
  }
}
