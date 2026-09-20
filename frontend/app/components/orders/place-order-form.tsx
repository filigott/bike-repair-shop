import { useTranslation } from "react-i18next";
import { Form, Link, useNavigation } from "react-router";
import { FormField } from "~/components/form-field";
import type { PlaceOrderRequest } from "~/services/endpoints/orders";
import type { ServiceTypeItem } from "~/services/endpoints/service-types";
import type { ActionFailure } from "~/types/action-result";

type PlaceOrderFormProps = {
  serviceTypes: ServiceTypeItem[];
  failure?: ActionFailure<keyof PlaceOrderRequest>;
};

export function PlaceOrderForm({ serviceTypes, failure }: PlaceOrderFormProps) {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const errors = failure?.fieldErrors ?? {};

  return (
    <Form method="post" className="flex max-w-xl flex-col gap-3">
      {failure && (
        <p role="alert" className="alert alert-error">
          {failure.detail ?? failure.title}
        </p>
      )}

      <FormField
        name="customerName"
        label={t("orders.fields.customerName")}
        errors={errors.customerName}
      >
        <input id="customerName" name="customerName" className="input w-full" />
      </FormField>

      <FormField
        name="phoneNumber"
        label={t("orders.fields.phoneNumber")}
        errors={errors.phoneNumber}
      >
        <input id="phoneNumber" name="phoneNumber" className="input w-full" />
      </FormField>

      <FormField
        name="email"
        label={t("orders.fields.email")}
        errors={errors.email}
      >
        <input id="email" name="email" className="input w-full" />
      </FormField>

      <FormField
        name="bikeBrand"
        label={t("orders.fields.bikeBrand")}
        errors={errors.bikeBrand}
      >
        <input id="bikeBrand" name="bikeBrand" className="input w-full" />
      </FormField>

      <FormField
        name="serviceType"
        label={t("orders.fields.serviceType")}
        errors={errors.serviceType}
      >
        <select id="serviceType" name="serviceType" className="select w-full">
          {serviceTypes.map((service) => (
            <option key={service.code} value={service.code}>
              {t(`orders.serviceType.${service.code}`)}
            </option>
          ))}
        </select>
      </FormField>

      <FormField
        name="dueDate"
        label={t("orders.fields.dueDate")}
        errors={errors.dueDate}
      >
        <input
          id="dueDate"
          name="dueDate"
          type="date"
          required
          className="input w-full"
        />
      </FormField>

      <FormField
        name="notes"
        label={t("orders.fields.notes")}
        errors={errors.notes}
      >
        <textarea id="notes" name="notes" className="textarea w-full" />
      </FormField>

      <div className="flex gap-2">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={navigation.state === "submitting"}
        >
          {t("orders.place.submit")}
        </button>
        <Link to="/" className="btn btn-ghost">
          {t("common.cancel")}
        </Link>
      </div>
    </Form>
  );
}
