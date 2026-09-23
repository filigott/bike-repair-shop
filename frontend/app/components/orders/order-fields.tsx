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
        <input
          id="customerName"
          name="customerName"
          defaultValue={defaults?.customerName}
          className="input w-full"
        />
      </FormField>

      <FormField
        name="phoneNumber"
        label={t("orders.fields.phoneNumber")}
        errors={errors.phoneNumber}
      >
        <input
          id="phoneNumber"
          name="phoneNumber"
          defaultValue={defaults?.phoneNumber}
          className="input w-full"
        />
      </FormField>

      <FormField
        name="email"
        label={t("orders.fields.email")}
        errors={errors.email}
      >
        <input
          id="email"
          name="email"
          defaultValue={defaults?.email}
          className="input w-full"
        />
      </FormField>

      <FormField
        name="bikeBrand"
        label={t("orders.fields.bikeBrand")}
        errors={errors.bikeBrand}
      >
        <input
          id="bikeBrand"
          name="bikeBrand"
          defaultValue={defaults?.bikeBrand}
          className="input w-full"
        />
      </FormField>

      <FormField
        name="notes"
        label={t("orders.fields.notes")}
        errors={errors.notes}
      >
        <textarea
          id="notes"
          name="notes"
          defaultValue={defaults?.notes ?? undefined}
          className="textarea w-full"
        />
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
      <select
        id="serviceType"
        name="serviceType"
        defaultValue={defaults?.serviceType}
        className="select w-full"
      >
        {serviceTypes.map((service) => (
          <option key={service.code} value={service.code}>
            {t(`orders.serviceType.${service.code}`)}
          </option>
        ))}
      </select>
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
      <input
        id="dueDate"
        name="dueDate"
        type="date"
        defaultValue={defaults?.dueDate}
        className="input w-full"
      />
    </FormField>
  );
}
