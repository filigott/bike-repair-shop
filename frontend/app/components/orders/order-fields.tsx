import { useTranslation } from "react-i18next";
import { FormField } from "~/components/form-field";
import type {
  MaintenanceOrderResponse,
  PlaceOrderRequest,
} from "~/services/endpoints/orders";
import type { ServiceTypeItem } from "~/services/endpoints/service-types";
import type { ValidationCode } from "~/types/problems";

export type OrderFormField = keyof PlaceOrderRequest;

export type OrderFieldErrors = Partial<
  Record<OrderFormField, ValidationCode[]>
>;

export type OrderDefaults = Partial<
  Pick<
    MaintenanceOrderResponse,
    | "customerName"
    | "phoneNumber"
    | "email"
    | "bikeBrand"
    | "serviceType"
    | "dueDate"
    | "notes"
  >
>;

export function OrderDetailsFields({
  defaults,
  errors,
}: {
  defaults?: OrderDefaults;
  errors: OrderFieldErrors;
}) {
  const { t } = useTranslation();

  return (
    <>
      <FormField
        name="customerName"
        label={t("orders.fields.customerName")}
        errors={errors.customerName}
      >
        {(control) => (
          <input
            {...control}
            autoComplete="name"
            defaultValue={defaults?.customerName}
            className="input w-full"
          />
        )}
      </FormField>

      <FormField
        name="phoneNumber"
        label={t("orders.fields.phoneNumber")}
        errors={errors.phoneNumber}
      >
        {(control) => (
          <input
            {...control}
            type="tel"
            autoComplete="tel"
            defaultValue={defaults?.phoneNumber}
            className="input w-full"
          />
        )}
      </FormField>

      <FormField
        name="email"
        label={t("orders.fields.email")}
        errors={errors.email}
      >
        {(control) => (
          <input
            {...control}
            inputMode="email"
            autoComplete="email"
            defaultValue={defaults?.email}
            className="input w-full"
          />
        )}
      </FormField>

      <FormField
        name="bikeBrand"
        label={t("orders.fields.bikeBrand")}
        errors={errors.bikeBrand}
      >
        {(control) => (
          <input
            {...control}
            defaultValue={defaults?.bikeBrand}
            className="input w-full"
          />
        )}
      </FormField>

      <FormField
        name="notes"
        label={t("orders.fields.notes")}
        errors={errors.notes}
      >
        {(control) => (
          <textarea
            {...control}
            defaultValue={defaults?.notes ?? undefined}
            className="textarea w-full"
          />
        )}
      </FormField>
    </>
  );
}

export function ServiceTypeField({
  serviceTypes,
  defaults,
  errors,
}: {
  serviceTypes: ServiceTypeItem[];
  defaults?: OrderDefaults;
  errors: OrderFieldErrors;
}) {
  const { t } = useTranslation();

  return (
    <FormField
      name="serviceType"
      label={t("orders.fields.serviceType")}
      errors={errors.serviceType}
    >
      {(control) => (
        <select
          {...control}
          defaultValue={defaults?.serviceType}
          className="select w-full"
        >
          {serviceTypes.map((service) => (
            <option key={service.code} value={service.code}>
              {t(`orders.serviceType.${service.code}`)}
            </option>
          ))}
        </select>
      )}
    </FormField>
  );
}

export function DueDateField({
  defaults,
  errors,
}: {
  defaults?: OrderDefaults;
  errors: OrderFieldErrors;
}) {
  const { t } = useTranslation();

  return (
    <FormField
      name="dueDate"
      label={t("orders.fields.dueDate")}
      errors={errors.dueDate}
    >
      {(control) => (
        <input
          {...control}
          type="date"
          defaultValue={defaults?.dueDate}
          className="input w-full"
        />
      )}
    </FormField>
  );
}
