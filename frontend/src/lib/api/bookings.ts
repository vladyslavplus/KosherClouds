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

export enum BookingZone {
  Terrace = 'Terrace',
  MainHall = 'MainHall',
  VIP = 'VIP',
}

export enum BookingStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Cancelled = 'Cancelled',
  Completed = 'Completed',
  NoShow = 'NoShow',
}

export enum HookahStrength {
  Light = 'Light',
  Medium = 'Medium',
  Strong = 'Strong',
}

export interface HookahBookingDto {
  productId?: string;
  productName?: string;
  productNameUk?: string;
  tobaccoFlavor: string;
  tobaccoFlavorUk?: string;
  strength: HookahStrength;
  serveAfterMinutes?: number | null;
  notes?: string | null;
  priceSnapshot?: number;
}

export interface BookingCreateDto {
  bookingDateTime: string;
  adults: number;
  children: number;
  zone: BookingZone;
  phoneNumber: string;
  comment?: string | null;
  hookahs?: HookahBookingDto[];
}

export interface BookingUpdateDto {
  bookingDateTime?: string | null;
  adults?: number | null;
  children?: number | null;
  zone?: BookingZone | null;
  phoneNumber?: string | null;
  comment?: string | null;
  hookahs?: HookahBookingDto[] | null;
  status?: BookingStatus | null;
}

export interface BookingResponse {
  id: string;
  bookingDateTime: string;
  adults: number;
  children: number;
  zone: BookingZone;
  status: BookingStatus;
  phoneNumber: string;
  comment?: string | null;
  userId: string;
  hookahs: HookahBookingDto[];
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

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface PagedBookingResponse {
  items: BookingResponse[];
  pagination: PaginationMetadata;
}

export const bookingsApi = {
  getBookings: async (params?: BookingParameters): Promise<PagedBookingResponse> => {
    const response = await apiClient.get<BookingResponse[]>('/bookings', { params });
    
    const paginationHeader = response.headers['x-pagination'];
    let pagination: PaginationMetadata;

    if (paginationHeader) {
      const parsed = JSON.parse(paginationHeader);

      pagination = {
        totalCount: parsed.totalCount ?? parsed.TotalCount,
        pageSize: parsed.pageSize ?? parsed.PageSize,
        currentPage: parsed.currentPage ?? parsed.CurrentPage,
        totalPages: parsed.totalPages ?? parsed.TotalPages,
        hasNext: parsed.hasNext ?? parsed.HasNext,
        hasPrevious: parsed.hasPrevious ?? parsed.HasPrevious,
      };
    } else {
      pagination = {
        totalCount: response.data.length,
        pageSize: params?.pageSize || 10,
        currentPage: params?.pageNumber || 1,
        totalPages: 1,
        hasNext: false,
        hasPrevious: false,
      };
    }

    return {
      items: response.data,
      pagination,
    };
  },

  getBookingById: async (bookingId: string): Promise<BookingResponse> => {
    const response = await apiClient.get<BookingResponse>(`/bookings/${bookingId}`);
    return response.data;
  },

  createBooking: async (data: BookingCreateDto): Promise<BookingResponse> => {
    const response = await apiClient.post<BookingResponse>('/bookings', data);
    return response.data;
  },

  updateBooking: async (bookingId: string, data: BookingUpdateDto): Promise<void> => {
    await apiClient.put(`/bookings/${bookingId}`, data);
  },

  cancelBooking: async (bookingId: string): Promise<void> => {
    await apiClient.post(`/bookings/${bookingId}/cancel`);
  },

  deleteBooking: async (bookingId: string): Promise<void> => {
    await apiClient.delete(`/bookings/${bookingId}`);
  },
};