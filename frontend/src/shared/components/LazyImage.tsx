import { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface LazyImageProps {
  src?: string;
  alt: string;
  className?: string;
}

export const LazyImage = ({ src, alt, className = '' }: LazyImageProps) => {
  const { t } = useTranslation();
  const [loaded, setLoaded] = useState(false);
  const [error, setError] = useState(false);

  const defaultImage = `data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="400" height="400" viewBox="0 0 400 400"%3E%3Crect fill="%23e5e7eb" width="400" height="400"/%3E%3Ctext x="50%25" y="50%25" dominant-baseline="middle" text-anchor="middle" font-family="sans-serif" font-size="24" fill="%239ca3af"%3E${t('common.noImage')}%3C/text%3E%3C/svg%3E`;

  if (!src || error) {
    return (
      <img
        src={defaultImage}
        alt={alt}
        className={className}
      />
    );
  }

  return (
    <>
      {!loaded && (
        <div className={`${className} animate-pulse bg-gray-300`} />
      )}
      <img
        src={src}
        alt={alt}
        className={`${className} ${loaded ? 'opacity-100' : 'opacity-0'} transition-opacity duration-300`}
        onLoad={() => setLoaded(true)}
        onError={() => setError(true)}
        loading="lazy"
      />
    </>
  );
};