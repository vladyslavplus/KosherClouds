import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Input } from '../../shared/ui/Input';
import { Button } from '../../shared/ui/Button';
import { authApi } from '../../lib/api/auth';
import { useAuthStore } from '../../lib/stores/authStore';

export function RegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);

  const [userName, setUserName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const { tokens, user } = await authApi.register({
        userName,
        phoneNumber,
        email,
        password,
      });
      setAuth(tokens.accessToken, tokens.refreshToken, user);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-12rem)] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md lg:max-w-lg xl:max-w-xl">
        <div className="bg-[#B4B6D4] rounded-[45px] shadow-lg p-8 sm:p-10 lg:p-12 xl:p-14">
          <h1 className="text-2xl sm:text-3xl lg:text-4xl font-heading font-bold text-center mb-8 lg:mb-10 text-[#000000]">
            {t('auth.register')}
          </h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-600 text-sm">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6 lg:space-y-7">
            <Input
              type="text"
              placeholder={t('auth.namePlaceholder')}
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              required
            />

            <Input
              type="tel"
              placeholder={t('auth.phonePlaceholder')}
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              required
            />

            <Input
              type="email"
              placeholder={t('auth.emailPlaceholder')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            <Input
              type="password"
              placeholder={t('auth.passwordPlaceholder')}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />

            <Button
              type="submit"
              variant="secondary"
              size="md"
              rounded="full"
              fullWidth
              isLoading={isLoading}
            >
              {t('auth.registerButton')}
            </Button>

            <div className="text-center">
              <span className="font-heading text-base lg:text-lg text-[#000000]">
                {t('auth.alreadyHaveAccount')}{' '}
              </span>
              <Link
                to="/login"
                className="font-heading font-semibold text-base lg:text-lg text-[#A67C1A] hover:text-[#8B6914] transition-colors"
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