import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ValidationCode } from "~/types/problems";

export type FieldControl = {
  id: string;
  name: string;
  "aria-invalid"?: true;
  "aria-describedby"?: string;
  onInput: () => void;
};

export function FormField({
  name,
  label,
  errors,
  children,
}: {
  name: string;
  label: string;
  errors?: ValidationCode[];
  children: (control: FieldControl) => React.ReactNode;
}) {
  const { t } = useTranslation();
  // Each rejected submit yields a fresh array, so an edit dismisses only the errors it followed.
  const [editedPast, setEditedPast] = useState<ValidationCode[]>();
  const shown = errors === editedPast ? undefined : errors;
  const errorId = `${name}-errors`;
  const invalid = Boolean(shown?.length);

  return (
    <div className="flex w-full flex-col gap-1">
      <label className="text-base font-medium" htmlFor={name}>
        {label}
      </label>
      {children({
        id: name,
        name,
        "aria-invalid": invalid || undefined,
        "aria-describedby": invalid ? errorId : undefined,
        onInput: () => setEditedPast(errors),
      })}
      {invalid && (
        <div id={errorId} className="flex flex-col text-error text-sm">
          {shown?.map((code) => (
            <span key={code}>{t(`validation.${code}`)}</span>
          ))}
        </div>
      )}
    </div>
  );
}
