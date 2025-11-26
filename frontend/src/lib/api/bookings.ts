import { apiClient } from './client';

export interface BookingParameters {
  userId?: string;
  minBookingDate?: string;
  maxBookingDate?: string;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  orderBy?: string;
}

export interface HookahBooking {
  tobaccoFlavor: string;
  strength: 'Light' | 'Medium' | 'Strong';
  serveAfterMinutes?: number | null;
  notes?: string | null;
}

export interface Booking {
  id: string;
  bookingDateTime: string;
  adults: number;
  children: number;
  zone: 'Terrace' | 'MainHall' | 'VIP';
  status: 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed' | 'NoShow';
  phoneNumber: string;
  comment?: string | null;
  userId: string;
  hookahs: HookahBooking[];
  createdAt: string;
  updatedAt?: string | null;
  totalGuests: number;
  hasHookahs: boolean;
  hookahCount: number;
  isUpcoming: boolean;
  isPast: boolean;
  canBeCancelled: boolean;
  canBeModified: boolean;
  displayBookingDate: string;
  displayBookingTime: string;
  displayBookingDateTime: string;
  displayGuestCount: string;
}

export const bookingsApi = {
  getBookings: async (params: BookingParameters = {}): Promise<Booking[]> => {
    const response = await apiClient.get<Booking[]>('/bookings', { params });
    return response.data;
  },

  getBookingById: async (bookingId: string): Promise<Booking> => {
    const response = await apiClient.get<Booking>(`/bookings/${bookingId}`);
    return response.data;
  },
};