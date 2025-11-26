import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Input } from '../../shared/ui/Input';
import { Button } from '../../shared/ui/Button';
import { Checkbox } from '../../shared/ui/Checkbox';
import { authApi } from '../../lib/api/auth';
import { useAuthStore } from '../../lib/stores/authStore';

export function LoginPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const { tokens, user } = await authApi.login({ email, password });
      setAuth(tokens.accessToken, tokens.refreshToken, user);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-[calc(100vh-12rem)] flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-md lg:max-w-lg xl:max-w-xl">
        <div className="bg-[#B4B6D4] rounded-[45px] shadow-lg p-8 sm:p-10 lg:p-12 xl:p-14">
          <h1 className="text-2xl sm:text-3xl lg:text-4xl font-heading font-bold text-center mb-8 lg:mb-10 text-[#000000]">
            {t('auth.login')}
          </h1>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-600 text-sm">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6 lg:space-y-7">
            <Input
              type="email"
              placeholder={t('auth.email')}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            <Input
              type="password"
              placeholder={t('auth.password')}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />

            <div className="pl-2">
              <Checkbox
                id="rememberMe"
                label={t('auth.rememberMe')}
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
              />
            </div>

            <Button
              type="submit"
              variant="secondary"
              size="md"
              rounded="full"
              fullWidth
              isLoading={isLoading}
            >
              {t('auth.loginButton')}
            </Button>

            <div className="text-center">
              <Link
                to="/forgot-password"
                className="font-heading font-semibold text-base lg:text-lg text-[#A67C1A] hover:text-[#8B6914] transition-colors"
              >
                {t('auth.forgotPassword')}
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}