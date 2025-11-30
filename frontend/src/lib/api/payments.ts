import { apiClient } from './client';

export interface PaymentRequestDto {
  orderId: string;
  amount: number;
  currency: string;
  receiptEmail: string;
}

export interface PaymentResponseDto {
  paymentId: string;
  status: string;
  transactionId?: string;
  clientSecret?: string;
  provider: string;
  createdAt: string;
}

export const paymentsApi = {
  createPayment: async (request: PaymentRequestDto): Promise<PaymentResponseDto> => {
    const response = await apiClient.post<PaymentResponseDto>('/payments', request);
    return response.data;
  },
};