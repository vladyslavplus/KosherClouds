import type { InputHTMLAttributes } from 'react';
import { cn } from '@/lib/utils/cn';

const CheckIcon = ({ className }: { className?: string }) => (
  <svg
    width="16"
    height="16"
    viewBox="0 0 16 16"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    className={cn("w-4 h-4", className)}
  >
    <path
      d="M13.3334 4L6.00002 11.3333L2.66669 8"
      stroke="white"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string;
  id: string;
  bordered?: boolean;
}

export function Checkbox({
  label,
  id,
  bordered = false,
  className,
  checked,
  ...props
}: CheckboxProps) {
  return (
    <label
      htmlFor={id}
      className={cn(
        'flex items-center gap-3 cursor-pointer group w-full',
        className
      )}
    >
      <input
        type="checkbox"
        id={id}
        checked={checked}
        className="sr-only peer"
        {...props}
      />

      <span
        className={cn(
          'relative flex items-center justify-center w-6 h-6 rounded transition-colors shrink-0',
          'border-2 border-transparent',
          'peer-checked:bg-[#0E1071]',
          'peer-focus-visible:ring-2 peer-focus-visible:ring-offset-2 peer-focus-visible:ring-[#0E1071]',
          'bg-white',
          bordered && 'border-[#000000] peer-checked:border-[#0E1071]'
        )}
      >
        <CheckIcon
          className={cn(
            "transition-opacity duration-200",
            checked ? "opacity-100" : "opacity-0"
          )}
        />
      </span>

      {label && (
        <span className="font-heading font-medium text-xl select-none leading-none">
          {label}
        </span>
      )}
    </label>
  );
}