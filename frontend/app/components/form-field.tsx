import { useTranslation } from "react-i18next";
import type { ValidationCode } from "~/types/problems";

export function FormField({
  name,
  label,
  errors,
  children,
}: {
  name: string;
  label: string;
  errors?: ValidationCode[];
  children: React.ReactNode;
}) {
  const { t } = useTranslation();

  return (
    <div className="flex w-full flex-col gap-1">
      <label className="text-base font-medium" htmlFor={name}>
        {label}
      </label>
      {children}
      {errors?.map((code) => (
        <span key={code} className="text-error text-sm">
          {t(`validation.${code}`)}
        </span>
      ))}
    </div>
  );
}
