import { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '../../lib/stores/authStore';
import { usersApi, type UserProfile } from '../../lib/api/users';
import { authApi } from '../../lib/api/auth';
import { Input } from '../../shared/ui/Input';
import { Button } from '../../shared/ui/Button';
import { EditableField } from '@/shared/components/EditableField';
import { OrdersTab } from './components/OrdersTab';
import { BookingsTab } from './components/BookingsTab';
import { AdminTab } from './components/AdminTab';

export function ProfilePage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { user: authUser, isAuthenticated, logout } = useAuthStore();

  const location = useLocation();
  const [activeTab, setActiveTab] = useState('profile');
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');

  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');

  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const isAdmin = authUser?.userName?.toLowerCase().includes('admin');

  const tabs = [
    { id: 'profile', label: t('profile.title') },
    { id: 'orders', label: t('profile.orders') },
    { id: 'bookings', label: t('profile.bookings') },
    ...(isAdmin ? [{ id: 'admin', label: t('profile.adminPanel') }] : []),
  ];

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    loadUser();
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    if (location.state?.tab) {
      setActiveTab(location.state.tab);
    }
  }, [location.state]);

  useEffect(() => {
    if (user) {
      setUserName(user.userName || '');
      setEmail(user.email || '');
      setPhoneNumber(user.phoneNumber || '');
    }
  }, [user]);

  const loadUser = async () => {
    setIsLoading(true);
    try {
      const userData = await usersApi.getCurrentUser();
      setUser(userData);
    } catch (err) {
      console.error('Failed to load user:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    if (!user) return;

    setError('');
    setSuccess('');
    setIsSaving(true);

    try {
      await usersApi.updateUser(user.id, {
        userName,
        email,
        phoneNumber,
      });
      setSuccess(t('profile.profileUpdated'));
      loadUser();
    } catch (err: any) {
      setError(err.response?.data?.message || t('profile.updateFailed'));
    } finally {
      setIsSaving(false);
    }
  };

  const handleChangePassword = async () => {
    if (!user) return;

    setError('');
    setSuccess('');
    setIsSaving(true);

    try {
      await usersApi.changePassword(user.id, {
        currentPassword,
        newPassword,
      });
      setSuccess(t('profile.passwordChanged'));
      setIsChangingPassword(false);
      setCurrentPassword('');
      setNewPassword('');
    } catch (err: any) {
      setError(err.response?.data?.message || t('profile.passwordChangeFailed'));
    } finally {
      setIsSaving(false);
    }
  };

  const handleLogout = async () => {
    try {
      await authApi.logout();
      logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  const handleTabChange = (tabId: string) => {
    setActiveTab(tabId);
    setIsMobileMenuOpen(false);
  };

  if (isLoading || !user) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#D9D9D9]">
        <p className="text-gray-600">{t('profile.loading')}</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#D9D9D9] py-4 md:py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="md:hidden mb-4">
          <div className="bg-white rounded-lg p-4 flex items-center justify-between">
            <h1 className="font-heading font-bold text-xl">
              {tabs.find(tab => tab.id === activeTab)?.label}
            </h1>
            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className="p-2 text-gray-600 hover:text-gray-900"
              aria-label="Toggle menu"
            >
              {isMobileMenuOpen ? (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              ) : (
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              )}
            </button>
          </div>
        </div>

        {isMobileMenuOpen && (
          <div className="md:hidden mb-4">
            <nav className="bg-white rounded-lg p-4">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => handleTabChange(tab.id)}
                  className={`w-full text-left px-4 py-3 font-heading font-semibold text-lg transition-colors mb-2 rounded-lg ${activeTab === tab.id
                    ? 'bg-[#1A1F3A] text-white'
                    : 'text-gray-400 hover:bg-gray-100'
                    }`}
                >
                  {tab.label}
                </button>
              ))}
            </nav>
          </div>
        )}

        <div className="flex flex-col md:flex-row gap-0 bg-white rounded-lg overflow-hidden">
          <aside className="hidden md:block w-64 shrink-0 border-r border-gray-200">
            <nav className="p-6">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`w-full text-left px-4 py-3 font-heading font-semibold text-xl transition-colors mb-2 ${activeTab === tab.id
                    ? 'text-[#000000]'
                    : 'text-gray-400'
                    }`}
                >
                  {tab.label}
                </button>
              ))}
            </nav>
          </aside>

          <main className="grow p-4 md:p-8">
            {activeTab === 'profile' && (
              <div className="max-w-2xl mx-auto">
                {error && (
                  <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
                    <p className="text-red-600 text-sm">{error}</p>
                  </div>
                )}

                {success && (
                  <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
                    <p className="text-green-600 text-sm">{success}</p>
                  </div>
                )}

                <div className="space-y-6">
                  <EditableField
                    label={t('profile.name')}
                    value={userName}
                    type="text"
                    onChange={setUserName}
                    disabled={isSaving}
                  />

                  <EditableField
                    label={t('profile.email')}
                    value={email}
                    type="email"
                    onChange={setEmail}
                    disabled={isSaving}
                  />

                  <EditableField
                    label={t('profile.phone')}
                    value={phoneNumber}
                    type="tel"
                    onChange={setPhoneNumber}
                    disabled={isSaving}
                  />

                  {!isChangingPassword ? (
                    <div className="flex items-center justify-between">
                      <button
                        onClick={() => setIsChangingPassword(true)}
                        className="font-heading font-medium text-base text-[#3A3DEF] hover:text-[#2e31c9] transition-colors hover:cursor-pointer"
                      >
                        {t('profile.changePasswordButton')}
                      </button>
                      <Button
                        variant="outline"
                        size="sm"
                        rounded="md"
                        onClick={handleLogout}
                        className="border-red-600! text-red-600! hover:bg-red-50!"
                      >
                        {t('profile.logoutButton')}
                      </Button>
                    </div>
                  ) : (
                    <div className="space-y-4 pt-4 border-t border-gray-300">
                      <h3 className="text-lg font-heading font-semibold">{t('profile.changePasswordTitle')}</h3>

                      <Input
                        label={t('profile.currentPassword')}
                        type="password"
                        value={currentPassword}
                        onChange={(e) => setCurrentPassword(e.target.value)}
                        rounded="full"
                        className="ring-2 ring-gray-300 focus:ring-[#3A3DEF]!"
                      />

                      <Input
                        label={t('profile.newPassword')}
                        type="password"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        rounded="full"
                        className="ring-2 ring-gray-300 focus:ring-[#3A3DEF]!"
                      />

                      <div className="flex flex-col sm:flex-row gap-3">
                        <Button
                          variant="primary"
                          size="sm"
                          rounded="md"
                          onClick={handleChangePassword}
                          isLoading={isSaving}
                          disabled={!currentPassword || !newPassword}
                          className="px-6! w-full sm:w-auto"
                        >
                          {t('profile.savePasswordButton')}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          rounded="md"
                          onClick={() => {
                            setIsChangingPassword(false);
                            setCurrentPassword('');
                            setNewPassword('');
                          }}
                          className="px-6! w-full sm:w-auto"
                        >
                          {t('profile.cancelButton')}
                        </Button>
                      </div>
                    </div>
                  )}

                  <div className="pt-4">
                    <Button
                      variant="primary"
                      size="md"
                      rounded="full"
                      fullWidth
                      onClick={handleSave}
                      isLoading={isSaving}
                    >
                      {t('profile.saveButton')}
                    </Button>
                  </div>
                </div>
              </div>
            )}

            {activeTab === 'orders' && <OrdersTab />}

            {activeTab === 'bookings' && <BookingsTab />}

            {activeTab === 'admin' && isAdmin && <AdminTab />}
          </main>
        </div>
      </div>
    </div>
  );
}