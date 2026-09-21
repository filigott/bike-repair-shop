export function FormField({
  name,
  label,
  errors,
  children,
}: {
  name: string;
  label: string;
  errors?: string[];
  children: React.ReactNode;
}) {
  return (
    <div className="flex w-full flex-col gap-1">
      <label className="text-base font-medium" htmlFor={name}>
        {label}
      </label>
      {children}
      {errors?.map((error) => (
        <span key={error} className="text-error text-sm">
          {error}
        </span>
      ))}
    </div>
  );
}
