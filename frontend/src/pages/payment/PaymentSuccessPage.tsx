import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ordersApi, OrderStatus } from '@/lib/api/orders';
import { Button } from '@/shared/ui/Button';

interface SuccessState {
  orderId: string;
  amount: number;
}

export default function PaymentSuccessPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as SuccessState | undefined;

  const [orderStatus, setOrderStatus] = useState<string>('');

  useEffect(() => {
    if (!state?.orderId) {
      navigate('/');
      return;
    }

    let interval: NodeJS.Timeout | null = null;

    const checkOrderStatus = async () => {
      try {
        const order = await ordersApi.getOrderById(state.orderId);
        setOrderStatus(order.status);

        if (order.status === OrderStatus.Paid) {
          if (interval) {
            clearInterval(interval);
          }
        }
      } catch (error) {
        console.error('Error checking order status:', error);
      }
    };

    checkOrderStatus();

    interval = setInterval(() => {
      checkOrderStatus();
    }, 3000);

    const timeout = setTimeout(() => {
      if (interval) {
        clearInterval(interval);
      }
    }, 30000);

    return () => {
      if (interval) {
        clearInterval(interval);
      }
      clearTimeout(timeout);
    };
  }, [state, navigate]);

  if (!state) {
    return null;
  }

  return (
    <div className="min-h-screen bg-[#F3F4F6] flex items-center justify-center py-12">
      <div className="max-w-md mx-auto px-4">
        <div className="bg-white rounded-lg shadow-lg p-8 text-center">
          {orderStatus === 'Paid' ? (
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </div>
          ) : (
            <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <div className="animate-spin rounded-full h-8 w-8 border-4 border-[#8B6914] border-t-transparent"></div>
            </div>
          )}

          <h1 className="text-3xl font-playfair font-bold mb-2 text-[#1A1F3A]">
            {orderStatus === 'Paid' ? t('payment.success.title') : t('payment.success.processing')}
          </h1>

          <p className="text-gray-600 mb-6">
            {orderStatus === 'Paid' ? t('payment.success.message') : t('payment.success.verifying')}
          </p>

          <div className="bg-gray-50 rounded-lg p-4 mb-6">
            <p className="text-sm text-gray-600">{t('payment.success.orderId')}</p>
            <p className="font-mono text-sm font-bold text-[#1A1F3A] mt-1 break-all">{state.orderId}</p>

            <p className="text-sm text-gray-600 mt-3">{t('payment.success.amount')}</p>
            <p className="text-2xl font-bold text-[#8B6914] mt-1 font-sans tabular-nums">{state.amount.toFixed(2)} ₴</p>
          </div>

          {orderStatus === 'Paid' && (
            <>
              <p className="text-sm text-orange-600 mb-4 font-semibold">
                {t('payment.success.pickupReminder')}
              </p>

              <p className="text-sm text-gray-600 mb-4">
                {t('payment.success.leaveReviewQuestion')}
              </p>

              <div className="space-y-3">
                <Button
                  onClick={() => navigate('/review', {
                    state: { orderId: state.orderId }
                  })}
                  className="w-full"
                >
                  {t('payment.success.leaveReview')}
                </Button>

                <button
                  onClick={() => navigate('/menu')}
                  className="w-full text-gray-600 hover:text-gray-800 font-medium"
                >
                  {t('payment.success.backToMenu')}
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}