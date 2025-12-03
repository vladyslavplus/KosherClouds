import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ordersApi, OrderResponseDto, PaymentType } from '@/lib/api/orders';
import { paymentsApi } from '@/lib/api/payments';
import { useAuthStore } from '@/lib/stores/authStore';
import { Button } from '@/shared/ui/Button';
import { Input } from '@/shared/ui/Input';
import { Select, SelectOption } from '@/shared/ui/Select';

interface CheckoutState {
  order: OrderResponseDto;
}

export default function CheckoutPage() {
  const { t, i18n } = useTranslation();
  const isUk = i18n.language === 'uk';
  const navigate = useNavigate();
  const location = useLocation();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const user = useAuthStore((state) => state.user);

  const state = location.state as CheckoutState | undefined;

  const [order, setOrder] = useState<OrderResponseDto | null>(state?.order ?? null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [processingPayment, setProcessingPayment] = useState(false);
  const [processingPickup, setProcessingPickup] = useState(false);

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

    if (state?.order) {
      setOrder(state.order);
      setContactName(state.order.contactName || '');
      setContactPhone(state.order.contactPhone || '');
      setLoading(false);
    } else {
      navigate('/cart');
    }
  }, [isAuthenticated, state, navigate]);

  const handleConfirmOrder = async () => {
    if (!order || !user) return;

    if (!contactPhone.trim()) {
      alert(t('checkout.phoneRequired'));
      return;
    }

    setSubmitting(true);
    try {
      const confirmedOrder = await ordersApi.confirmOrder(order.id, {
        contactName: contactName.trim(),
        contactPhone: contactPhone.trim(),
        notes: notes.trim() || undefined,
        paymentType: paymentType,
      });

      if (paymentType === PaymentType.Online) {
        setProcessingPayment(true);
        await new Promise(resolve => setTimeout(resolve, 2000));

        const uahToUsd = 0.024;
        const amountInUsd = confirmedOrder.totalAmount * uahToUsd;

        const paymentResponse = await paymentsApi.createPayment({
          orderId: confirmedOrder.id,
          amount: amountInUsd,
          currency: 'usd',
          receiptEmail: user.email || confirmedOrder.contactEmail,
        });

        if (!paymentResponse.clientSecret) {
          throw new Error('No client secret received');
        }

        navigate('/payment', {
          state: {
            clientSecret: paymentResponse.clientSecret,
            orderId: confirmedOrder.id,
            amount: confirmedOrder.totalAmount,
            amountUsd: amountInUsd,
          }
        });

        return;
      }

      setProcessingPickup(true);

      await new Promise(resolve => setTimeout(resolve, 2000));

      navigate('/profile', { state: { tab: 'orders' } });

    } catch (error) {
      console.error('Error confirming order:', error);
      alert(t('checkout.error'));
      setProcessingPickup(false);
    } finally {
      setSubmitting(false);
    }
  };

  const handleBackToCart = async () => {
    if (order?.status === 'Draft') {
      try {
        await ordersApi.deleteDraftOrder(order.id);
      } catch (error) {
        console.error('Error deleting draft order:', error);
      }
    }
    navigate('/cart');
  };

  if (loading) {
    return (
      <Loader message={t('checkout.processingOrder')} />
    );
  }

  if (processingPayment) {
    return (
      <Loader 
        message={t('checkout.processingOrder')} 
        sub={t('checkout.redirectingToPayment')} 
      />
    );
  }

  if (processingPickup) {
    return (
      <Loader 
        message={t('checkout.processingPickupOrder')} 
        sub={t('checkout.redirectingToOrders')} 
      />
    );
  }

  if (!order) return null;

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

            <h2 className="text-xl font-semibold mb-4 text-[#1A1F3A]">
              {t('checkout.contactInfo')}
            </h2>

            <div className="space-y-4 mb-8">
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
            </div>

            <div className="border-t-2 border-gray-100 my-8"></div>

            <h2 className="text-xl font-semibold mb-4 text-[#1A1F3A]">
              {t('checkout.orderNotes')}
            </h2>

            <textarea
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder={t('checkout.orderNotesPlaceholder')}
              rows={4}
              className="w-full px-6 py-3 border-2 border-[#000000] rounded-2xl resize-none"
            />

            <p className="text-sm text-orange-600 mt-2 font-semibold">
              ⚠️ {t('checkout.pickupOnlyWarning')}
            </p>

            <div className="border-t-2 border-gray-100 my-8"></div>

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
                        <p className="font-semibold text-[#1A1F3A]">{itemName}</p>
                        <p className="text-sm text-gray-600">
                          {item.quantity} × {item.unitPriceSnapshot.toFixed(2)} {t('menu.uah')}
                        </p>
                      </div>
                      <p className="font-semibold text-[#1A1F3A]">
                        {item.lineTotal.toFixed(2)} {t('menu.uah')}
                      </p>
                    </div>
                  );
                })}
              </div>

              <div className="border-t-2 border-gray-200 pt-4 mb-4">
                <div className="flex justify-between items-center mb-4">
                  <span className="text-xl font-semibold">{t('checkout.total')}</span>
                  <span className="text-2xl font-bold text-[#8B6914]">
                    {totalAmount.toFixed(2)} {t('menu.uah')}
                  </span>
                </div>

                <div className="flex justify-between items-center text-gray-600 text-sm">
                  <span>{t('checkout.paymentMethod')}:</span>
                  <span className="font-semibold">{selectedPaymentLabel}</span>
                </div>
              </div>

              <Button
                variant="primary"
                fullWidth
                onClick={handleConfirmOrder}
                isLoading={submitting}
                disabled={!contactName.trim() || !contactPhone.trim()}
              >
                {t('checkout.confirmOrder')}
              </Button>

              <button
                onClick={handleBackToCart}
                className="w-full mt-4 text-gray-600 hover:text-gray-800 font-medium"
              >
                {t('checkout.backToCart')}
              </button>

            </div>
          </div>

        </div>
      </div>
    </div>
  );
}

function Loader({ message, sub }: { message: string; sub?: string }) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-[#F3F4F6]">
      <div className="text-center">
        <div className="inline-block animate-spin rounded-full h-16 w-16 border-4 border-[#8B6914] border-t-transparent mb-4"></div>
        <h2 className="text-2xl font-playfair font-bold text-[#1A1F3A] mb-2">
          {message}
        </h2>
        {sub && <p className="text-gray-600">{sub}</p>}
      </div>
    </div>
  );
}
