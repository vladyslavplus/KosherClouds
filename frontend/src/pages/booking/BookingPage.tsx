import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { bookingsApi, BookingCreateDto, BookingZone, HookahBookingDto } from '../../lib/api/bookings';
import { Container } from '@/shared/layouts';
import { Button } from '@/shared/ui/Button';
import { Input } from '@/shared/ui/Input';
import { Select, SelectOption } from '@/shared/ui/Select';
import { HookahModal } from './components/HookahModal';

function BookingPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const isUk = i18n.language === 'uk';

  const [formData, setFormData] = useState<{
    bookingDateTime: string;
    adults: number;
    children: number;
    zone: BookingZone;
    phoneNumber: string;
    comment: string;
    hookahs: (HookahBookingDto & { price: number })[];
  }>({
    bookingDateTime: '',
    adults: 1,
    children: 0,
    zone: BookingZone.MainHall,
    phoneNumber: '',
    comment: '',
    hookahs: [],
  });

  const [fieldErrors, setFieldErrors] = useState<{ phoneNumber?: string; bookingDateTime?: string }>({});
  const [isHookahModalOpen, setIsHookahModalOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const zoneOptions: SelectOption[] = [
    { value: BookingZone.MainHall, label: t('booking.zones.mainHall') },
    { value: BookingZone.Terrace, label: t('booking.zones.terrace') },
    { value: BookingZone.VIP, label: t('booking.zones.vip') },
  ];

  const validatePhone = (phone: string): boolean => {
    const phoneRegex = /^\+?[1-9]\d{9,14}$/;
    if (!phone) {
      setFieldErrors(prev => ({ ...prev, phoneNumber: t('booking.errors.phoneRequired') }));
      return false;
    }
    if (!phoneRegex.test(phone)) {
      setFieldErrors(prev => ({ ...prev, phoneNumber: t('booking.errors.phoneInvalid') }));
      return false;
    }
    setFieldErrors(prev => ({ ...prev, phoneNumber: undefined }));
    return true;
  };

  const validateDate = (dateStr: string): boolean => {
    if (!dateStr) {
      setFieldErrors(prev => ({ ...prev, bookingDateTime: t('booking.errors.dateRequired') }));
      return false;
    }
    const date = new Date(dateStr);
    const now = new Date();
    if (date <= now) {
      setFieldErrors(prev => ({ ...prev, bookingDateTime: t('booking.errors.dateFuture') }));
      return false;
    }
    setFieldErrors(prev => ({ ...prev, bookingDateTime: undefined }));
    return true;
  };

  const handlePhoneBlur = () => {
    validatePhone(formData.phoneNumber);
  };

  const handleDateBlur = () => {
    validateDate(formData.bookingDateTime);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const isPhoneValid = validatePhone(formData.phoneNumber);
    const isDateValid = validateDate(formData.bookingDateTime);

    if (!isPhoneValid || !isDateValid) {
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const bookingData: BookingCreateDto = {
        bookingDateTime: formData.bookingDateTime,
        adults: formData.adults,
        children: formData.children,
        zone: formData.zone,
        phoneNumber: formData.phoneNumber,
        comment: formData.comment || null,
        hookahs: formData.hookahs.length > 0
          ? formData.hookahs.map(h => ({
            productId: h.productId,
            productName: h.productName,
            productNameUk: h.productNameUk,
            tobaccoFlavor: h.tobaccoFlavor,
            tobaccoFlavorUk: h.tobaccoFlavorUk,
            strength: h.strength,
            serveAfterMinutes: h.serveAfterMinutes,
            notes: h.notes,
            priceSnapshot: h.priceSnapshot,
          }))
          : undefined,
      };

      await bookingsApi.createBooking(bookingData);
      navigate('/profile', { state: { tab: 'bookings' } });

    } catch (err: any) {
      setError(err.response?.data?.message || t('booking.error'));
    } finally {
      setLoading(false);
    }
  };

  const handleAddHookah = (hookah: HookahBookingDto & { price: number }) => {
    setFormData(prev => ({
      ...prev,
      hookahs: [...prev.hookahs, hookah],
    }));
    setIsHookahModalOpen(false);
  };

  const handleRemoveHookah = (index: number) => {
    setFormData(prev => ({
      ...prev,
      hookahs: prev.hookahs.filter((_, i) => i !== index),
    }));
  };

  const totalHookahPrice = formData.hookahs.reduce((sum, h) => sum + h.price, 0);

  const getHookahDisplayName = (hookah: HookahBookingDto & { price: number }): string => {
    if (isUk && hookah.productNameUk) return hookah.productNameUk;
    return hookah.productName || hookah.tobaccoFlavor;
  };

  const getHookahDisplayFlavor = (hookah: HookahBookingDto & { price: number }): string => {
    if (isUk && hookah.tobaccoFlavorUk) return hookah.tobaccoFlavorUk;
    return hookah.tobaccoFlavor;
  };

  return (
    <main className="grow py-8">
      <Container>
        <div className="max-w-2xl mx-auto">
          <h1 className="text-4xl font-['Poltawski_Nowy'] font-semibold text-primary mb-8">
            {t('booking.title')}
          </h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-600">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-sm font-medium mb-2">
                {t('booking.dateTime')}
              </label>
              <Input
                type="datetime-local"
                value={formData.bookingDateTime}
                onChange={(e) => {
                  setFormData(prev => ({ ...prev, bookingDateTime: e.target.value }));
                  if (fieldErrors.bookingDateTime) validateDate(e.target.value);
                }}
                onBlur={handleDateBlur}
                required
                bordered
                lang={isUk ? 'uk-UA' : 'en-GB'}
                className={fieldErrors.bookingDateTime ? 'border-red-500 focus:ring-red-500' : ''}
              />
              {fieldErrors.bookingDateTime && (
                <p className="mt-1 text-sm text-red-600">{fieldErrors.bookingDateTime}</p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('booking.adults')}
                </label>
                <Input
                  type="number"
                  min="1"
                  max="50"
                  value={formData.adults}
                  onChange={(e) => setFormData(prev => ({ ...prev, adults: parseInt(e.target.value) }))}
                  required
                  bordered
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('booking.children')}
                </label>
                <Input
                  type="number"
                  min="0"
                  max="50"
                  value={formData.children}
                  onChange={(e) => setFormData(prev => ({ ...prev, children: parseInt(e.target.value) }))}
                  required
                  bordered
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {t('booking.zone')}
              </label>
              <Select
                options={zoneOptions}
                value={formData.zone}
                onChange={(value) => setFormData(prev => ({ ...prev, zone: value as BookingZone }))}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {t('booking.phoneNumber')}
              </label>
              <Input
                type="tel"
                value={formData.phoneNumber}
                onChange={(e) => {
                  setFormData(prev => ({ ...prev, phoneNumber: e.target.value }));
                  if (fieldErrors.phoneNumber) validatePhone(e.target.value);
                }}
                onBlur={handlePhoneBlur}
                placeholder="+380123456789"
                required
                bordered
                className={fieldErrors.phoneNumber ? 'border-red-500 focus:ring-red-500' : ''}
              />
              {fieldErrors.phoneNumber && (
                <p className="mt-1 text-sm text-red-600">{fieldErrors.phoneNumber}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">
                {t('booking.comment')} ({t('booking.optional')})
              </label>
              <textarea
                value={formData.comment || ''}
                onChange={(e) => setFormData(prev => ({ ...prev, comment: e.target.value }))}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent resize-none"
                rows={4}
                maxLength={1000}
              />
            </div>

            <div className="border-t pt-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold">{t('booking.hookahPreOrder')}</h3>
                {formData.hookahs.length < 5 && (
                  <Button
                    type="button"
                    variant="secondary"
                    size="sm"
                    onClick={() => setIsHookahModalOpen(true)}
                  >
                    {t('booking.addHookah')}
                  </Button>
                )}
              </div>

              {formData.hookahs.length > 0 && (
                <div className="space-y-3">
                  {formData.hookahs.map((hookah, index) => (
                    <div key={index} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg border border-gray-200">
                      <div className="flex-1">
                        <p className="font-medium">
                          {getHookahDisplayName(hookah)}
                        </p>
                        <p className="text-sm text-gray-600">
                          {getHookahDisplayFlavor(hookah)}
                        </p>
                        <p className="text-sm text-gray-600">
                          {t('booking.strength')}: {t(`booking.strengthLevels.${hookah.strength.toLowerCase()}`)}
                        </p>
                        {hookah.serveAfterMinutes != null && hookah.serveAfterMinutes > 0 && (
                          <p className="text-sm text-gray-600">
                            {t('booking.serveAfter')}: {hookah.serveAfterMinutes} {t('booking.minutes')}
                          </p>
                        )}
                        {hookah.notes && (
                          <p className="text-sm text-gray-600">{hookah.notes}</p>
                        )}
                        <p className="text-sm font-semibold text-secondary mt-1 tabular-nums">
                          {hookah.price} {isUk ? 'грн' : 'uah'}
                        </p>
                      </div>
                      <button
                        type="button"
                        onClick={() => handleRemoveHookah(index)}
                        className="text-red-600 hover:text-red-700 ml-4"
                      >
                        {t('booking.remove')}
                      </button>
                    </div>
                  ))}
                  <div className="text-right pt-2 border-t">
                    <p className="text-lg font-semibold tabular-nums">
                      {t('booking.totalHookahPrice')}: {totalHookahPrice} {isUk ? 'грн' : 'uah'}
                    </p>
                  </div>
                </div>
              )}
            </div>

            <Button
              type="submit"
              variant="primary"
              fullWidth
              disabled={loading}
            >
              {loading ? t('booking.submitting') : t('booking.submit')}
            </Button>
          </form>
        </div>
      </Container>

      <HookahModal
        isOpen={isHookahModalOpen}
        onClose={() => setIsHookahModalOpen(false)}
        onConfirm={handleAddHookah}
      />
    </main>
  );
}

export default BookingPage;