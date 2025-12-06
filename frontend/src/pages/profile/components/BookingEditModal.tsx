import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { bookingsApi, BookingResponse, BookingStatus, BookingZone, HookahStrength } from '@/lib/api/bookings';
import { Button } from '@/shared/ui/Button';
import { Select, SelectOption } from '@/shared/ui/Select';
import CloseIcon from '@/assets/icons/close.svg?react';

interface BookingEditModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  booking: BookingResponse | null;
}

export function BookingEditModal({ isOpen, onClose, onSuccess, booking }: BookingEditModalProps) {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [bookingDateTime, setBookingDateTime] = useState('');
  const [adults, setAdults] = useState(1);
  const [children, setChildren] = useState(0);
  const [zone, setZone] = useState<BookingZone>(BookingZone.MainHall);
  const [phoneNumber, setPhoneNumber] = useState('');
  const [status, setStatus] = useState<BookingStatus>(BookingStatus.Pending);

  const zoneOptions: SelectOption[] = [
    { value: BookingZone.Terrace, label: t('booking.zones.terrace') },
    { value: BookingZone.MainHall, label: t('booking.zones.mainHall') },
    { value: BookingZone.VIP, label: t('booking.zones.vip') },
  ];

  const statusOptions: SelectOption[] = [
    { value: BookingStatus.Pending, label: t('admin.bookings.status.pending') },
    { value: BookingStatus.Confirmed, label: t('admin.bookings.status.confirmed') },
    { value: BookingStatus.Cancelled, label: t('admin.bookings.status.cancelled') },
    { value: BookingStatus.Completed, label: t('admin.bookings.status.completed') },
    { value: BookingStatus.NoShow, label: t('admin.bookings.status.noshow') },
  ];

  const getStrengthLabel = (strength: HookahStrength): string => {
    switch (strength) {
      case HookahStrength.Light:
        return t('booking.strengthLevels.light');
      case HookahStrength.Medium:
        return t('booking.strengthLevels.medium');
      case HookahStrength.Strong:
        return t('booking.strengthLevels.strong');
      default:
        return strength;
    }
  };

  useEffect(() => {
    if (booking) {
      const date = new Date(booking.bookingDateTime);
      const localDateTime = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 16);
      
      setBookingDateTime(localDateTime);
      setAdults(booking.adults);
      setChildren(booking.children);
      setZone(booking.zone);
      setPhoneNumber(booking.phoneNumber);
      setStatus(booking.status);
    }
  }, [booking]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!booking) return;

    setError(null);
    setLoading(true);

    try {
      const updateData = {
        bookingDateTime: new Date(bookingDateTime).toISOString(),
        adults,
        children,
        zone,
        phoneNumber,
        status,
      };

      await bookingsApi.updateBooking(booking.id, updateData);
      setError(null);
      onSuccess();
    } catch (err: any) {
      setError(err.response?.data?.message || t('admin.bookingEditModal.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setError(null);
    onClose();
  };

  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      handleClose();
    }
  };

  if (!isOpen || !booking) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm bg-black/50 p-4"
      onClick={handleBackdropClick}
    >
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        <div className="border-b border-gray-200 px-5 py-3 sticky top-0 bg-white">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-heading font-semibold">
              {t('admin.bookingEditModal.title')}
            </h2>
            <button onClick={handleClose} className="p-1.5 hover:bg-gray-100 rounded-full transition-colors">
              <CloseIcon className="w-5 h-5 text-gray-600" />
            </button>
          </div>
        </div>

        <div className="p-5">
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-600 text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">
                {t('admin.bookingEditModal.bookingDateTime')}
              </label>
              <input
                type="datetime-local"
                value={bookingDateTime}
                onChange={(e) => setBookingDateTime(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#8B6914]"
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.bookingEditModal.adults')}
                </label>
                <input
                  type="number"
                  min="1"
                  max="20"
                  value={adults}
                  onChange={(e) => setAdults(parseInt(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#8B6914]"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.bookingEditModal.children')}
                </label>
                <input
                  type="number"
                  min="0"
                  max="20"
                  value={children}
                  onChange={(e) => setChildren(parseInt(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#8B6914]"
                  required
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {t('admin.bookingEditModal.zone')}
              </label>
              <Select
                options={zoneOptions}
                value={zone}
                onChange={(value) => setZone(value as BookingZone)}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {t('admin.bookingEditModal.phoneNumber')}
              </label>
              <input
                type="tel"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#8B6914]"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {t('admin.bookingEditModal.status')}
              </label>
              <Select
                options={statusOptions}
                value={status}
                onChange={(value) => setStatus(value as BookingStatus)}
              />
            </div>

            {booking.comment && (
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.bookingEditModal.comment')} {t('admin.bookingEditModal.commentReadonly')}
                </label>
                <div className="p-3 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-700">
                  {booking.comment}
                </div>
              </div>
            )}

            {booking.hasHookahs && (
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('admin.bookingEditModal.hookahs')} {t('admin.bookingEditModal.hookahsReadonly')}
                </label>
                <div className="space-y-2">
                  {booking.hookahs.map((hookah, index) => (
                    <div key={index} className="p-3 bg-gray-50 border border-gray-200 rounded-lg text-sm">
                      <div className="font-medium">
                        {isUk ? hookah.productNameUk || hookah.productName : hookah.productName}
                      </div>
                      <div className="text-gray-600 mt-1">
                        {isUk ? hookah.tobaccoFlavorUk || hookah.tobaccoFlavor : hookah.tobaccoFlavor} • {getStrengthLabel(hookah.strength)}
                      </div>
                      {hookah.notes && (
                        <div className="text-gray-500 text-xs mt-1">{hookah.notes}</div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}

            <div className="flex gap-3 pt-3">
              <Button type="button" variant="outline" fullWidth onClick={handleClose} size="sm">
                {t('admin.bookingEditModal.cancel')}
              </Button>
              <Button type="submit" variant="primary" fullWidth isLoading={loading} size="sm">
                {t('admin.bookingEditModal.save')}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}