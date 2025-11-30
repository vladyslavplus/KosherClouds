import { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { loadStripe } from '@stripe/stripe-js';
import { Elements } from '@stripe/react-stripe-js';
import { CheckoutForm } from './components/CheckoutForm';

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLIC_KEY);

interface PaymentState {
  clientSecret: string;
  orderId: string;
  amount: number;
  amountUsd: number;
}

export default function PaymentPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as PaymentState | undefined;

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!state?.clientSecret || !state?.orderId) {
      navigate('/cart');
      return;
    }
    setLoading(false);
  }, [state, navigate]);

  if (loading || !state) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#F3F4F6]">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-[#8B6914] border-t-transparent"></div>
      </div>
    );
  }

  const appearance = {
    theme: 'stripe' as const,
    variables: {
      colorPrimary: '#8B6914',
      colorBackground: '#ffffff',
      colorText: '#1A1F3A',
      colorDanger: '#df1b41',
      fontFamily: 'system-ui, sans-serif',
      spacingUnit: '4px',
      borderRadius: '12px',
    },
  };

  const options = {
    clientSecret: state.clientSecret,
    appearance,
    developerTools: {
      assistant: false
    },
  };

  return (
    <div className="min-h-screen bg-[#F3F4F6] py-12">
      <div className="max-w-2xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-lg p-8">
          <h1 className="text-3xl font-playfair font-bold mb-2 text-[#1A1F3A]">
            {t('payment.title')}
          </h1>
          <p className="text-gray-600 mb-2">
            {t('payment.amount')}: <span className="font-bold text-[#8B6914] font-sans tabular-nums">{state.amount.toFixed(2)}₴</span>
          </p>
          <p className="text-sm text-gray-500 mb-6">
            {t('payment.amountUsd')}: <span className="font-semibold font-sans tabular-nums">${state.amountUsd.toFixed(2)}</span>
          </p>

          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
            <p className="text-sm text-blue-800 font-medium">
              {t('payment.testMode')}
            </p>
            <p className="text-xs text-blue-600 mt-1">
              {t('payment.testCard')}: <code className="bg-white px-2 py-1 rounded font-mono">4242 4242 4242 4242</code>
            </p>
          </div>

          <Elements stripe={stripePromise} options={options}>
            <CheckoutForm orderId={state.orderId} amount={state.amount} amountUsd={state.amountUsd} />
          </Elements>
        </div>
      </div>
    </div>
  );
}