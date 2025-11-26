import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Input } from '../../shared/ui/Input';
import { Button } from '../../shared/ui/Button';
import { authApi } from '../../lib/api/auth';

export function ForgotPasswordPage() {
  const { t } = useTranslation();

  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess(false);
    setIsLoading(true);

    try {
      await authApi.forgotPassword({ email });
      setSuccess(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send reset email. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-12rem)] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md lg:max-w-lg xl:max-w-xl">
        <div className="bg-[#B4B6D4] rounded-[45px] shadow-lg p-8 sm:p-10 lg:p-12 xl:p-14">
          <h1 className="text-2xl sm:text-3xl lg:text-4xl font-heading font-bold text-center mb-8 lg:mb-10 text-[#000000]">
            {t('auth.forgotPasswordTitle')}
          </h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-600 text-sm">{error}</p>
            </div>
          )}

          {success && (
            <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
              <p className="text-green-600 text-sm">
                Password reset link has been sent to your email if it exists.
              </p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6 lg:space-y-7">
            <Input
              type="email"
              placeholder={t('auth.emailPlaceholder')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            <Button
              type="submit"
              variant="primary"
              size="md"
              rounded="full"
              fullWidth
              isLoading={isLoading}
            >
              {t('auth.sendButton')}
            </Button>

            <div className="text-center">
              <Link
                to="/login"
                className="font-heading font-semibold text-base lg:text-lg text-[#A67C1A] hover:text-[#8B6914] transition-colors"
              >
                {t('auth.backToLogin')}
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}