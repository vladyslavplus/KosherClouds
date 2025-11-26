import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Input } from '../../shared/ui/Input';
import { Button } from '../../shared/ui/Button';
import { authApi } from '../../lib/api/auth';

export function ResetPasswordPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const token = searchParams.get('token');
  const email = searchParams.get('email');

  useEffect(() => {
    if (!token || !email) {
      setError('Invalid reset link. Please request a new password reset.');
    }
  }, [token, email]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    if (!token || !email) {
      setError('Invalid reset link.');
      return;
    }

    setIsLoading(true);

    try {
      await authApi.resetPassword({
        email,
        token,
        newPassword,
        confirmPassword,
      });
      navigate('/login');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-12rem)] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md lg:max-w-lg xl:max-w-xl">
        <div className="bg-[#B4B6D4] rounded-[45px] shadow-lg p-8 sm:p-10 lg:p-12 xl:p-14">
          <h1 className="text-2xl sm:text-3xl lg:text-4xl font-heading font-bold text-center mb-8 lg:mb-10 text-[#000000]">
            {t('auth.resetPasswordTitle')}
          </h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-600 text-sm">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6 lg:space-y-7">
            <Input
              type="password"
              placeholder={t('auth.newPasswordPlaceholder')}
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />

            <Input
              type="password"
              placeholder={t('auth.confirmPasswordPlaceholder')}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />

            <Button
              type="submit"
              variant="primary"
              size="md"
              rounded="full"
              fullWidth
              isLoading={isLoading}
              disabled={!token || !email}
            >
              {t('auth.savePasswordButton')}
            </Button>

            <div className="text-center">
              <span className="font-heading text-base lg:text-lg text-[#000000]">
                {t('auth.rememberPassword')}{' '}
              </span>
              <Link
                to="/login"
                className="font-heading font-semibold text-base lg:text-lg text-[#000000] hover:text-[#1A1F3A] transition-colors"
              >
                {t('auth.signIn')}
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}