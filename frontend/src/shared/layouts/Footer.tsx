import { useTranslation } from 'react-i18next';

export function Footer() {
  const { t } = useTranslation();

  return (
    <footer className="bg-[#262B4D] text-white">
      <div className="w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-8">
          
          <div className="space-y-2">
            <p className="font-heading text-lg">
              {t('footer.city')}
            </p>
            <p className="font-heading text-lg">
              {t('footer.street')}
            </p>
          </div>

          <div className="space-y-2 text-left md:text-right">
            <p className="font-heading text-lg">
              {t('footer.phone')}
            </p>
            <p className="font-heading text-lg">
              {t('footer.email')}
            </p>
            <p className="font-heading text-lg cursor-pointer hover:text-gray-300 transition-colors">
              {t('footer.privacy')}
            </p>
          </div>
        </div>
      </div>
    </footer>
  );
}