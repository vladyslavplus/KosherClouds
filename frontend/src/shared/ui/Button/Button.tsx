import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '@/lib/utils/cn';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  variant?: 'primary' | 'secondary' | 'outline' | 'auth' | 'status-green' | 'status-orange' | 'status-blue' | 'status-red';
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  isLoading?: boolean;
  icon?: ReactNode;
  rounded?: 'full' | 'lg' | 'md' | 'sm' | 'none' | '20';
  interactive?: boolean;
}

export function Button({
  children,
  variant = 'primary',
  size = 'md',
  fullWidth = false,
  isLoading = false,
  icon,
  rounded = 'full',
  interactive = true,
  className,
  disabled,
  ...props
}: ButtonProps) {
  const baseStyles = 'font-heading transition-all duration-200';
  
  const variants = {
    primary: 'bg-[#1A1F3A] text-white hover:bg-[#252b47] active:scale-[0.98] font-bold',
    secondary: 'bg-[#8B6914] text-white hover:bg-[#9d7518] active:scale-[0.98] font-bold',
    outline: 'bg-transparent border-2 border-[#000000] text-[#000000] hover:bg-gray-50 active:scale-[0.98] font-medium',
    auth: 'bg-[#0E1071] text-white hover:bg-[#0c0d5f] active:scale-[0.98] font-normal',
    'status-green': 'bg-[#15803D] text-white border border-[#000000] font-medium',
    'status-orange': 'bg-[#D97706] text-white border border-[#000000] font-medium',
    'status-blue': 'bg-[#3E4373] text-white border border-[#000000] font-medium',
    'status-red': 'bg-[#B91C1C] text-white border border-[#000000] font-medium',
  };

  const interactiveStyles = interactive
    ? 'cursor-pointer hover:opacity-80 active:scale-[0.98]'
    : 'cursor-default';

  const disabledStyles = interactive ? 'disabled:opacity-50 disabled:cursor-not-allowed' : '';

  const sizes = {
    sm: 'px-4 py-1.5 text-base',
    md: 'px-8 py-3 text-2xl',
    lg: 'px-10 py-4 text-3xl',
  };

  const roundedStyles = {
    full: 'rounded-[45px]',
    lg: 'rounded-2xl',
    md: 'rounded-lg',
    sm: 'rounded-md',
    none: 'rounded-none',
    '20': 'rounded-[20px]',
  };

  return (
    <button
      className={cn(
        baseStyles,
        variants[variant],
        sizes[size],
        roundedStyles[rounded],
        interactiveStyles,
        disabledStyles,
        fullWidth && 'w-full',
        isLoading && 'opacity-70 cursor-wait',
        className
      )}
      disabled={interactive ? (disabled || isLoading) : true}
      onClick={interactive ? props.onClick : undefined}
      {...props}
    >
      {isLoading ? (
        <span className="flex items-center justify-center gap-2">
          <svg
            className="animate-spin h-5 w-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          {children}
        </span>
      ) : (
        <span className="flex items-center justify-center gap-3">
          {icon && <span className="shrink-0">{icon}</span>}
          {children}
        </span>
      )}
    </button>
  );
}