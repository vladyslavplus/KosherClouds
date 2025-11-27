import { useState, useRef, useEffect } from 'react';
import { cn } from '@/lib/utils/cn';

const ChevronDownIcon = ({ stroke = "#374151" }: { stroke?: string }) => (
  <svg 
    width="20" 
    height="20" 
    viewBox="0 0 20 20" 
    fill="none" 
    xmlns="http://www.w3.org/2000/svg"
    className="w-5 h-5"
  >
    <path 
      d="M5 7.5L10 12.5L15 7.5" 
      stroke={stroke} 
      strokeWidth="2" 
      strokeLinecap="round" 
      strokeLinejoin="round"
    />
  </svg>
);

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps {
  options: SelectOption[];
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  className?: string;
  rounded?: 'full' | 'lg' | 'md' | 'sm' | 'none';
  disabled?: boolean;
  iconStroke?: string;
}

export function Select({
  options,
  value: controlledValue,
  defaultValue,
  onChange,
  placeholder = 'Оберіть...',
  className,
  rounded = 'full',
  disabled = false,
  iconStroke = '#374151',
}: SelectProps) {
  const [internalValue, setInternalValue] = useState(defaultValue || '');
  const [isOpen, setIsOpen] = useState(false);
  const selectRef = useRef<HTMLDivElement>(null);
  
  const value = controlledValue !== undefined ? controlledValue : internalValue;
  const selectedOption = options.find(opt => opt.value === value);
  
  const roundedStyles = {
    full: 'rounded-[45px]',
    lg: 'rounded-2xl',
    md: 'rounded-lg',
    sm: 'rounded-md',
    none: 'rounded-none',
  };
  
  const handleSelect = (optionValue: string) => {
    if (controlledValue === undefined) {
      setInternalValue(optionValue);
    }
    onChange?.(optionValue);
    setIsOpen(false);
  };
  
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (selectRef.current && !selectRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    
    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);
  
  return (
    <div ref={selectRef} className={cn('relative inline-block', className)}>
      <button
        type="button"
        onClick={() => !disabled && setIsOpen(!isOpen)}
        disabled={disabled}
        className={cn(
          'flex items-center justify-between gap-4 px-6 py-3 min-w-[250px]',
          'border-2 border-[#000000] bg-white',
          'font-heading font-medium text-lg text-[#4B5563]',
          'transition-all duration-200',
          'hover:bg-gray-50 active:scale-[0.98]',
          'disabled:opacity-50 disabled:cursor-not-allowed',
          'focus:outline-none',
          roundedStyles[rounded]
        )}
      >
        <span className="truncate">
          {selectedOption ? selectedOption.label : placeholder}
        </span>
        <ChevronDownIcon stroke={iconStroke} />
      </button>
      
      {isOpen && (
        <div
          className={cn(
            'absolute z-50 w-full mt-2 py-2',
            'bg-white border-2 border-[#000000]',
            'shadow-lg',
            'max-h-[300px] overflow-y-auto',
            // Приховати scrollbar на великих екранах (md і більше)
            'md:[&::-webkit-scrollbar]:hidden md:scrollbar-hide',
            // Показати scrollbar тільки на маленьких екранах
            '[&::-webkit-scrollbar]:w-2',
            '[&::-webkit-scrollbar-track]:bg-gray-100',
            '[&::-webkit-scrollbar-track]:rounded-full',
            '[&::-webkit-scrollbar-thumb]:bg-gray-300',
            '[&::-webkit-scrollbar-thumb]:rounded-full',
            '[&::-webkit-scrollbar-thumb]:hover:bg-gray-400',
            roundedStyles[rounded]
          )}
          style={{
            // Для Firefox
            scrollbarWidth: window.innerWidth >= 768 ? 'none' : 'thin',
          }}
        >
          {options.map((option) => (
            <button
              key={option.value}
              type="button"
              onClick={() => handleSelect(option.value)}
              className={cn(
                'w-full px-6 py-3 text-left',
                'font-heading font-medium text-lg',
                'transition-colors duration-150',
                'hover:bg-gray-100',
                value === option.value
                  ? 'bg-gray-50 text-[#000000]'
                  : 'text-[#4B5563]'
              )}
            >
              {option.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}