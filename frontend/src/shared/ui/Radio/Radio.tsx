import type { InputHTMLAttributes } from 'react';
import { cn } from '@/lib/utils/cn';

export interface RadioProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string;
  id: string;
}

export function Radio({
  label,
  id,
  className,
  checked,
  ...props
}: RadioProps) {
  return (
    <label
      htmlFor={id}
      className={cn(
        'inline-flex items-center gap-3 cursor-pointer group',
        className
      )}
    >
      <input
        type="radio"
        id={id}
        checked={checked}
        className="sr-only peer"
        {...props}
      />
      
      <span
        className={cn(
          'relative flex items-center justify-center w-6 h-6 rounded-full border border-[#000000] transition-all',
          'peer-checked:bg-[#3B82F6] peer-checked:border-[#000000]',
          'peer-focus-visible:ring-2 peer-focus-visible:ring-offset-2 peer-focus-visible:ring-[#3B82F6]',
          'bg-white'
        )}
      >
        {checked && (
          <span className="w-2.5 h-2.5 rounded-full bg-white" />
        )}
      </span>

      <span className="font-heading font-medium text-xl select-none">
        {label}
      </span>
    </label>
  );
}