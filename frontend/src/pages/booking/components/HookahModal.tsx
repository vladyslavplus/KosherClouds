import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { productsApi, ProductResponse, ProductCategory } from '@/lib/api/products';
import { HookahBookingDto, HookahStrength } from '@/lib/api/bookings';
import { Button } from '@/shared/ui/Button';
import { Select, SelectOption } from '@/shared/ui/Select';
import { Input } from '@/shared/ui/Input';
import { LazyImage } from '@/shared/components/LazyImage';
import CloseIcon from '@/assets/icons/close.svg?react';

interface HookahModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: (hookah: HookahBookingDto & { price: number }) => void;
}

export const HookahModal = ({ isOpen, onClose, onConfirm }: HookahModalProps) => {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';

  const [hookahs, setHookahs] = useState<ProductResponse[]>([]);
  const [selectedHookah, setSelectedHookah] = useState<ProductResponse | null>(null);
  const [strength, setStrength] = useState<HookahStrength>(HookahStrength.Medium);
  const [serveAfterMinutes, setServeAfterMinutes] = useState<string>('');
  const [notes, setNotes] = useState<string>('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      fetchHookahs();
    }
  }, [isOpen]);

  const fetchHookahs = async () => {
    setLoading(true);
    try {
      const response = await productsApi.getProducts({
        category: ProductCategory.Hookah,
        isAvailable: true,
        pageSize: 100,
      });
      setHookahs(response.items);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const strengthOptions: SelectOption[] = [
    { value: HookahStrength.Light, label: t('booking.strengthLevels.light') },
    { value: HookahStrength.Medium, label: t('booking.strengthLevels.medium') },
    { value: HookahStrength.Strong, label: t('booking.strengthLevels.strong') },
  ];

  const handleConfirm = () => {
    if (!selectedHookah || !selectedHookah.hookahDetails) return;

    const minutes = serveAfterMinutes === '' ? null : parseInt(serveAfterMinutes);

    const hookahData: HookahBookingDto & { price: number } = {
      productId: selectedHookah.id,
      productName: selectedHookah.name,
      productNameUk: selectedHookah.nameUk || undefined,
      tobaccoFlavor: selectedHookah.hookahDetails.tobaccoFlavor,
      tobaccoFlavorUk: selectedHookah.hookahDetails.tobaccoFlavorUk || undefined,
      strength,
      serveAfterMinutes: minutes,
      notes: notes.trim() || null,
      priceSnapshot: selectedHookah.discountPrice || selectedHookah.price,
      price: selectedHookah.discountPrice || selectedHookah.price,
    };

    onConfirm(hookahData);
    resetForm();
  };

  const resetForm = () => {
    setSelectedHookah(null);
    setStrength(HookahStrength.Medium);
    setServeAfterMinutes('');
    setNotes('');
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  const handleBackdropClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      handleClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm"
      onClick={handleBackdropClick}
    >
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-semibold">{t('booking.selectHookah')}</h2>
            <button
              onClick={handleClose}
              className="p-2 hover:bg-gray-100 rounded-full transition-colors"
            >
              <CloseIcon className="w-6 h-6 text-gray-600" />
            </button>
          </div>

          {loading ? (
            <div className="text-center py-8">{t('common.loading')}</div>
          ) : (
            <div className="space-y-6">
              <div>
                <label className="block text-sm font-medium mb-2">
                  {t('booking.hookahFlavor')}
                </label>
                <div className="grid grid-cols-1 gap-3">
                  {hookahs.map((hookah) => (
                    <button
                      key={hookah.id}
                      type="button"
                      onClick={() => setSelectedHookah(hookah)}
                      className={`p-4 border rounded-lg text-left transition-all ${
                        selectedHookah?.id === hookah.id
                          ? 'border-secondary bg-secondary/10'
                          : 'border-gray-300 hover:border-secondary/50'
                      }`}
                    >
                      <div className="flex items-center gap-4">
                        <LazyImage
                          src={hookah.photos[0]}
                          alt={hookah.name}
                          className="w-16 h-16 object-cover rounded"
                        />
                        <div className="flex-1">
                          <p className="font-medium">
                            {isUk && hookah.nameUk ? hookah.nameUk : hookah.name}
                          </p>
                          <p className="text-sm text-gray-600">
                            {hookah.hookahDetails && (
                              isUk && hookah.hookahDetails.tobaccoFlavorUk
                                ? hookah.hookahDetails.tobaccoFlavorUk
                                : hookah.hookahDetails.tobaccoFlavor
                            )}
                          </p>
                          <p className="text-sm font-semibold text-secondary mt-1 tabular-nums">
                            {hookah.discountPrice || hookah.price} {isUk ? 'грн' : 'uah'}
                          </p>
                        </div>
                      </div>
                    </button>
                  ))}
                </div>
              </div>

              {selectedHookah && (
                <>
                  <div>
                    <label className="block text-sm font-medium mb-2">
                      {t('booking.strength')}
                    </label>
                    <Select
                      options={strengthOptions}
                      value={strength}
                      onChange={(value) => setStrength(value as HookahStrength)}
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      {t('booking.serveAfter')} ({t('booking.optional')})
                    </label>
                    <Input
                      type="number"
                      min="0"
                      max="240"
                      value={serveAfterMinutes}
                      onChange={(e) => setServeAfterMinutes(e.target.value)}
                      placeholder={t('booking.minutesPlaceholder')}
                      bordered
                    />
                    {serveAfterMinutes === '' && (
                      <p className="text-sm text-gray-500 mt-1">{t('booking.immediatelyNote')}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      {t('booking.notes')} ({t('booking.optional')})
                    </label>
                    <textarea
                      value={notes}
                      onChange={(e) => setNotes(e.target.value)}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-secondary focus:border-transparent resize-none"
                      rows={3}
                      maxLength={500}
                    />
                  </div>

                  <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                    <p className="text-lg font-semibold">
                      {t('booking.selected')}: {isUk && selectedHookah.nameUk ? selectedHookah.nameUk : selectedHookah.name}
                    </p>
                    <p className="text-2xl font-bold text-secondary mt-2 tabular-nums">
                      {selectedHookah.discountPrice || selectedHookah.price} {isUk ? 'грн' : 'uah'}
                    </p>
                  </div>
                </>
              )}

              <div className="flex gap-4">
                <Button
                  type="button"
                  variant="outline"
                  fullWidth
                  onClick={handleClose}
                >
                  {t('booking.cancel')}
                </Button>
                <Button
                  type="button"
                  variant="primary"
                  fullWidth
                  onClick={handleConfirm}
                  disabled={!selectedHookah}
                >
                  {t('booking.confirm')}
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};