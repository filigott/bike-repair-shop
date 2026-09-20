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
    <div className="form-control w-full">
      <label className="label-text" htmlFor={name}>
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
