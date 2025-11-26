import type { InputHTMLAttributes } from 'react';
import { cn } from '@/lib/utils/cn';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  rounded?: 'full' | 'lg' | 'md' | 'sm' | 'none';
  bordered?: boolean;
}

export function Input({
  label,
  error,
  rounded = 'full',
  bordered = false,
  className,
  id,
  ...props
}: InputProps) {
  const roundedStyles = {
    full: 'rounded-[45px]',
    lg: 'rounded-2xl',
    md: 'rounded-lg',
    sm: 'rounded-md',
    none: 'rounded-none',
  };

  const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;

  return (
    <div className="w-full">
      {label && (
        <label
          htmlFor={inputId}
          className="block mb-2 font-heading font-medium text-base text-[#000000]"
        >
          {label}
        </label>
      )}

      <input
        id={inputId}
        className={cn(
          'w-full px-6 py-3',
          'bg-white',
          'font-heading font-medium text-lg text-[#000000] placeholder:text-[#B4B6D4]',
          'transition-all duration-200',
          'focus:outline-none',
          'disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-gray-50',
          bordered && 'border-2 border-[#000000] focus:border-[#000000]',
          roundedStyles[rounded],
          error && 'border-2 border-red-500',
          className
        )}
        {...props}
      />

      {error && (
        <p className="mt-1.5 text-sm text-red-600 font-sans">
          {error}
        </p>
      )}
    </div>
  );
}