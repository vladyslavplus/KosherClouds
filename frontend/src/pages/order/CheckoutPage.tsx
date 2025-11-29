import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ordersApi, OrderResponseDto, PaymentType } from '@/lib/api/orders';
import { useAuthStore } from '@/lib/stores/authStore';
import { Button } from '@/shared/ui/Button';
import { Input } from '@/shared/ui/Input';
import { Select, SelectOption } from '@/shared/ui/Select';

export default function CheckoutPage() {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';
  const navigate = useNavigate();
  const location = useLocation();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  const [order, setOrder] = useState<OrderResponseDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  const [contactName, setContactName] = useState('');
  const [contactPhone, setContactPhone] = useState('');
  const [notes, setNotes] = useState('');
  const [paymentType, setPaymentType] = useState<PaymentType>(PaymentType.OnPickup);

  const paymentOptions: SelectOption[] = [
    { value: PaymentType.OnPickup, label: t('checkout.paymentOnPickup') },
    { value: PaymentType.Online, label: t('checkout.paymentOnline') },
  ];

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }

    const stateOrder = location.state?.order as OrderResponseDto | undefined;

    if (stateOrder) {
      setOrder(stateOrder);

      setContactName(stateOrder.contactName || '');
      setContactPhone(stateOrder.contactPhone || '');

      setLoading(false);
    } else {
      navigate('/cart');
    }
  }, [isAuthenticated, location.state, navigate]);

  const handleConfirmOrder = async () => {
    if (!order) return;

    if (!contactPhone.trim()) {
      alert(t('checkout.phoneRequired'));
      return;
    }

    setSubmitting(true);
    try {
      await ordersApi.confirmOrder(order.id, {
        contactName: contactName.trim(),
        contactPhone: contactPhone.trim(),
        notes: notes.trim() || undefined,
        paymentType: paymentType,
      });

      navigate('/orders', {
        state: { message: t('checkout.orderConfirmed') }
      });
    } catch (error) {
      console.error('Error confirming order:', error);
      alert(t('checkout.error'));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#F3F4F6]">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
      </div>
    );
  }

  if (!order) {
    return null;
  }

  const totalAmount = order.totalAmount;
  const selectedPaymentLabel = paymentOptions.find(opt => opt.value === paymentType)?.label || '';

  return (
    <div className="min-h-screen bg-[#F3F4F6] py-8">
      <div className="max-w-6xl mx-auto px-4">
        <h1 className="text-3xl font-playfair font-bold mb-8 text-[#1A1F3A]">
          {t('checkout.title')}
        </h1>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <div className="bg-white rounded-lg p-6">
            <div className="mb-8">
              <h2 className="text-xl font-semibold mb-4 text-[#1A1F3A]">
                {t('checkout.contactInfo')}
              </h2>

              <div className="space-y-4">
                <Input
                  label={t('checkout.name')}
                  value={contactName}
                  onChange={(e) => setContactName(e.target.value)}
                  placeholder={t('checkout.namePlaceholder')}
                  bordered
                  rounded="full"
                  required
                />

                <Input
                  label={t('checkout.email')}
                  type="email"
                  value={order.contactEmail}
                  disabled
                  bordered
                  rounded="full"
                />

                <Input
                  label={t('checkout.phone')}
                  type="tel"
                  value={contactPhone}
                  onChange={(e) => setContactPhone(e.target.value)}
                  placeholder={t('checkout.phonePlaceholder')}
                  bordered
                  rounded="full"
                  required
                />

                <div className="pt-2">
                  <p className="text-sm text-gray-600">
                    {t('checkout.contactInfoNote')}
                  </p>
                </div>
              </div>
            </div>

            <div className="border-t-2 border-gray-100 my-8"></div>

            <div className="mb-8">
              <h2 className="text-xl font-semibold mb-4 text-[#1A1F3A]">
                {t('checkout.orderNotes')}
              </h2>

              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder={t('checkout.orderNotesPlaceholder')}
                rows={4}
                maxLength={500}
                className="w-full px-6 py-3 border-2 border-[#000000] rounded-2xl font-heading font-medium text-lg text-[#000000] placeholder:text-[#B4B6D4] focus:outline-none resize-none"
              />

              <p className="text-sm text-orange-600 mt-2 font-semibold">
                ⚠️ {t('checkout.pickupOnlyWarning')}
              </p>
            </div>

            <div className="border-t-2 border-gray-100 my-8"></div>

            <div>
              <h2 className="text-xl font-semibold mb-4 text-[#1A1F3A]">
                {t('checkout.payment')}
              </h2>

              <Select
                options={paymentOptions}
                value={paymentType}
                onChange={(value) => setPaymentType(value as PaymentType)}
                rounded="full"
                className="w-full"
              />
            </div>
          </div>

          <div>
            <div className="bg-white rounded-lg p-6 sticky top-4">
              <h2 className="text-xl font-semibold mb-6 text-[#1A1F3A]">
                {t('checkout.yourOrder')}
              </h2>

              <div className="space-y-4 mb-6">
                {order.items.map((item) => {
                  const itemName = isUk && item.productNameSnapshotUk
                    ? item.productNameSnapshotUk
                    : item.productNameSnapshot;

                  return (
                    <div key={item.id} className="flex justify-between items-start">
                      <div className="flex-1">
                        <p className="font-semibold text-[#1A1F3A]">
                          {itemName}
                        </p>
                        <p className="text-sm text-gray-600">
                          {item.quantity} × {item.unitPriceSnapshot.toFixed(2)} {t('menu.uah')}
                        </p>
                      </div>
                      <p className="font-semibold text-[#1A1F3A] font-sans tabular-nums">
                        {item.lineTotal.toFixed(2)} {t('menu.uah')}
                      </p>
                    </div>
                  );
                })}
              </div>

              <div className="border-t-2 border-gray-200 pt-4 mb-4">
                <div className="flex justify-between items-center mb-4">
                  <span className="text-xl font-semibold text-[#1A1F3A]">
                    {t('checkout.total')}
                  </span>
                  <span className="text-2xl font-bold text-[#8B6914] font-sans tabular-nums">
                    {totalAmount.toFixed(2)} {t('menu.uah')}
                  </span>
                </div>

                <div className="flex justify-between items-center text-gray-600">
                  <span className="text-sm">
                    {t('checkout.paymentMethod')}:
                  </span>
                  <span className="text-sm font-semibold">
                    {selectedPaymentLabel}
                  </span>
                </div>
              </div>

              <Button
                variant="primary"
                size="sm"
                rounded="full"
                fullWidth
                onClick={handleConfirmOrder}
                isLoading={submitting}
                disabled={!contactName.trim() || !contactPhone.trim()}
              >
                {t('checkout.confirmOrder')}
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}