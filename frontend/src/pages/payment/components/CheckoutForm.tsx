import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  useStripe,
  useElements,
  PaymentElement,
} from '@stripe/react-stripe-js';
import { Button } from '@/shared/ui/Button';
import { ordersApi, OrderStatus } from '@/lib/api/orders';

interface CheckoutFormProps {
  orderId: string;
  amount: number;
  amountUsd: number;
}

export function CheckoutForm({ orderId, amount, amountUsd }: CheckoutFormProps) {
  const { t } = useTranslation();
  const stripe = useStripe();
  const elements = useElements();
  const navigate = useNavigate();
  const [isProcessing, setIsProcessing] = useState(false);
  const [isCanceling, setIsCanceling] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);
    setErrorMessage(null);

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: `${window.location.origin}/payment-success?orderId=${orderId}`,
      },
      redirect: 'if_required',
    });

    if (error) {
      setErrorMessage(error.message || t('payment.errorGeneric'));
      setIsProcessing(false);
    } else {
      navigate('/payment-success', {
        state: { orderId, amount }
      });
    }
  };

  const handleCancel = async () => {
    setIsCanceling(true);
    setErrorMessage(null);

    try {
      await ordersApi.updateOrder(orderId, {
        status: OrderStatus.Canceled
      });
      navigate('/checkout');
    } catch (error) {
      console.error('Error canceling order:', error);
      setErrorMessage(t('payment.errorCanceling'));
      setIsCanceling(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <PaymentElement />
      {errorMessage && (
        <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-800">{errorMessage}</p>
        </div>
      )}
      <Button
        type="submit"
        disabled={!stripe || isProcessing}
        className="w-full mt-6 font-sans tabular-nums"
      >
        {isProcessing 
          ? t('payment.processing') 
          : `${t('payment.pay')} ${amount.toFixed(2)} ₴ ($${amountUsd.toFixed(2)})`
        }
      </Button>
      <button
        type="button"
        onClick={handleCancel}
        disabled={isCanceling}
        className="w-full mt-3 text-gray-600 hover:text-gray-800 font-medium disabled:text-gray-400"
      >
        {isCanceling ? t('payment.canceling') : t('payment.cancel')}
      </button>
    </form>
  );
}