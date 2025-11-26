import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Button } from '../ui/Button';
import { useAuthStore } from '../../lib/stores/authStore';
import LogoSvg from '../../assets/images/logo.svg?url';
import UserIcon from '../../assets/icons/user.svg?url';
import CartIcon from '../../assets/icons/cart.svg?url';

export function Navbar() {
  const { t, i18n } = useTranslation();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  const toggleLanguage = () => {
    i18n.changeLanguage(i18n.language === 'en' ? 'uk' : 'en');
  };

  return (
    <nav className="bg-white border-b border-gray-200">
      <div className="w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-24">
          
          <Link to="/" className="flex items-center shrink-0">
            <img 
              src={LogoSvg} 
              alt="Kosher Clouds" 
              className="h-16 w-16 sm:h-20 sm:w-20 lg:h-24 lg:w-24 xl:h-28 xl:w-28"
            />
          </Link>

          <div className="hidden md:flex items-center gap-8 lg:gap-16 xl:gap-20 2xl:gap-24">
            <Link 
              to="/menu" 
              className="font-heading font-medium text-xl text-[#000000] hover:text-[#0E1071] transition-colors whitespace-nowrap"
            >
              {t('nav.menu')}
            </Link>
            <Link 
              to="/booking" 
              className="font-heading font-medium text-xl text-[#000000] hover:text-[#0E1071] transition-colors whitespace-nowrap"
            >
              {t('nav.booking')}
            </Link>
            <Link 
              to="/reviews" 
              className="font-heading font-medium text-xl text-[#000000] hover:text-[#0E1071] transition-colors whitespace-nowrap"
            >
              {t('nav.reviews')}
            </Link>
          </div>

          <div className="flex items-center gap-3 lg:gap-4">
            <button
              onClick={toggleLanguage}
              className="px-3 py-1 font-heading font-medium text-xl text-gray-700 hover:text-[#0E1071] transition-colors"
            >
              {i18n.language === 'en' ? 'UA' : 'EN'}
            </button>

            {!isAuthenticated ? (
              <>
                <Link to="/login">
                  <Button 
                    variant="auth" 
                    size="sm" 
                    rounded="20" 
                    className="min-w-[100px] lg:min-w-[110px] xl:min-w-[120px] text-xl! py-2! px-6!"
                  >
                    {t('auth.login')}
                  </Button>
                </Link>
                <Link to="/register">
                  <Button 
                    variant="auth" 
                    size="sm" 
                    rounded="20" 
                    className="min-w-[150px] lg:min-w-40 xl:min-w-[170px] text-xl! py-2! px-6!"
                  >
                    {t('auth.register')}
                  </Button>
                </Link>
              </>
            ) : (
              <>
                <Link to="/cart" className="hover:opacity-70 transition-opacity">
                  <img src={CartIcon} alt="Cart" className="h-8 w-8" />
                </Link>
                <Link to="/profile" className="hover:opacity-70 transition-opacity">
                  <img src={UserIcon} alt="Profile" className="h-8 w-8" />
                </Link>
              </>
            )}
          </div>
        </div>

        <div className="md:hidden flex items-center justify-center gap-6 pb-3 border-t border-gray-100 pt-3">
          <Link 
            to="/menu" 
            className="font-heading font-medium text-lg text-[#000000] hover:text-[#0E1071] transition-colors"
          >
            {t('nav.menu')}
          </Link>
          <Link 
            to="/booking" 
            className="font-heading font-medium text-lg text-[#000000] hover:text-[#0E1071] transition-colors"
          >
            {t('nav.booking')}
          </Link>
          <Link 
            to="/reviews" 
            className="font-heading font-medium text-lg text-[#000000] hover:text-[#0E1071] transition-colors"
          >
            {t('nav.reviews')}
          </Link>
        </div>
      </div>
    </nav>
  );
}