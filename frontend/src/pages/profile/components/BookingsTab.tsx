import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { bookingsApi, BookingResponse } from '@/lib/api/bookings';
import { Button } from '@/shared/ui/Button';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Pagination } from '@/shared/ui/Pagination';

export function BookingsTab() {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';

  const [bookings, setBookings] = useState<BookingResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [sortBy, setSortBy] = useState('bookingDateTime desc');

  const pageSize = 3;

  const sortOptions: SelectOption[] = [
    { value: 'bookingDateTime desc', label: t('bookings.sortBy.newest') },
    { value: 'bookingDateTime asc', label: t('bookings.sortBy.oldest') }
  ];

  useEffect(() => {
    fetchBookings();
  }, [currentPage, sortBy]);

  const fetchBookings = async () => {
    setLoading(true);
    try {
      const response = await bookingsApi.getBookings({
        pageNumber: currentPage,
        pageSize,
        orderBy: sortBy,
      });
      setBookings(response.items);
      setTotalPages(response.pagination.totalPages);
    } catch (error) {
      console.error('Error fetching bookings:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelBooking = async (bookingId: string) => {
    try {
      await bookingsApi.cancelBooking(bookingId);
      fetchBookings();
    } catch (error) {
      console.error(error);
    }
  };

  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr);
    return date.toLocaleDateString(isUk ? 'uk-UA' : 'en-GB', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const formatTime = (dateStr: string): string => {
    const date = new Date(dateStr);
    return date.toLocaleTimeString(isUk ? 'uk-UA' : 'en-GB', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'Confirmed':
        return 'text-green-600 bg-green-50';
      case 'Pending':
        return 'text-yellow-600 bg-yellow-50';
      case 'Cancelled':
        return 'text-red-600 bg-red-50';
      case 'Completed':
        return 'text-blue-600 bg-blue-50';
      case 'NoShow':
        return 'text-gray-600 bg-gray-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  };

  const getZoneLabel = (zone: string): string => {
    const zoneMap: Record<string, string> = {
      'MainHall': 'mainHall',
      'Terrace': 'terrace',
      'VIP': 'vip',
    };
    return t(`booking.zones.${zoneMap[zone] || zone.toLowerCase()}`);
  };

  const getHookahDisplayName = (hookah: any): string => {
    if (isUk && hookah.productNameUk) return hookah.productNameUk;
    return hookah.productName || hookah.tobaccoFlavor;
  };

  const getHookahDisplayFlavor = (hookah: any): string => {
    if (isUk && hookah.tobaccoFlavorUk) return hookah.tobaccoFlavorUk;
    return hookah.tobaccoFlavor;
  };

  if (loading && bookings.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">{t('common.loading')}</p>
      </div>
    );
  }

  if (!loading && bookings.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">{t('bookings.noBookings')}</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold">{t('profile.bookings')}</h2>
        <div className="w-64">
          <Select
            options={sortOptions}
            value={sortBy}
            onChange={setSortBy}
            placeholder={t('bookings.sortBy.label')}
          />
        </div>
      </div>

      <div className="space-y-4">
        {bookings.map((booking) => (
          <div
            key={booking.id}
            className="bg-white border border-gray-200 rounded-lg p-6 space-y-4"
          >
            <div className="flex items-start justify-between">
              <div className="space-y-1">
                <p className="text-sm text-gray-500">
                  {t('bookings.bookingNumber')}: <span className="font-mono">{booking.id.slice(0, 8)}</span>
                </p>
                <p className="text-lg font-semibold text-gray-900">
                  {formatDate(booking.bookingDateTime)} {t('bookings.at')} {formatTime(booking.bookingDateTime)}
                </p>
                <p className="text-sm text-gray-600">
                  {t('bookings.createdOn')}: {formatDate(booking.createdAt)}
                </p>
              </div>
              <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(booking.status)}`}>
                {t(`bookings.status.${booking.status.toLowerCase()}`)}
              </span>
            </div>

            <div className="border-t pt-4 grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-500">{t('booking.zone')}</p>
                <p className="font-medium">{getZoneLabel(booking.zone)}</p>
              </div>
              <div>
                <p className="text-sm text-gray-500">{t('booking.guests')}</p>
                <p className="font-medium">
                  {booking.adults}{t('booking.adultsCount', { count: booking.adults })}
                  {booking.children > 0 && `, ${booking.children}${t('booking.childrenCount', { count: booking.children })}`}
                </p>
              </div>
              <div>
                <p className="text-sm text-gray-500">{t('booking.phoneNumber')}</p>
                <p className="font-medium">{booking.phoneNumber}</p>
              </div>
              {booking.comment && (
                <div className="col-span-2">
                  <p className="text-sm text-gray-500">{t('booking.comment')}</p>
                  <p className="text-gray-700">{booking.comment}</p>
                </div>
              )}
            </div>

            {booking.hasHookahs && booking.hookahs.length > 0 && (
              <div className="border-t pt-4">
                <p className="text-sm font-medium mb-3">
                  {t('booking.hookahPreOrder')} ({booking.hookahCount})
                </p>
                <div className="space-y-3">
                  {booking.hookahs.map((hookah, index) => (
                    <div
                      key={index}
                      className="bg-gray-50 rounded-lg p-3 space-y-1"
                    >
                      <div className="flex justify-between items-start">
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">
                            {getHookahDisplayName(hookah)}
                          </p>
                          <p className="text-sm text-gray-600">
                            {getHookahDisplayFlavor(hookah)}
                          </p>
                        </div>
                        <span className="text-sm text-gray-600">
                          {t(`booking.strengthLevels.${hookah.strength.toLowerCase()}`)}
                        </span>
                      </div>
                        {hookah.serveAfterMinutes != null && hookah.serveAfterMinutes > 0 && (
                            <p className="text-sm text-gray-600">
                                {t('booking.serveAfter')}: {hookah.serveAfterMinutes} {t('booking.minutes')}
                            </p>
                        )}
                      {hookah.notes && (
                        <p className="text-sm text-gray-600">
                          {t('booking.notes')}: {hookah.notes}
                        </p>
                      )}
                      {hookah.priceSnapshot && (
                        <p className="text-sm font-semibold text-secondary tabular-nums">
                          {hookah.priceSnapshot} {isUk ? 'грн' : 'uah'}
                        </p>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {booking.canBeCancelled && (
              <div className="border-t pt-4 flex justify-end">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleCancelBooking(booking.id)}
                >
                  {t('bookings.cancelBooking')}
                </Button>
              </div>
            )}
          </div>
        ))}
      </div>

      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={setCurrentPage}
      />
    </div>
  );
}