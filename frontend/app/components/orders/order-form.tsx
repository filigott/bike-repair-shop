import { useTranslation } from "react-i18next";
import { Form, Link, useNavigation } from "react-router";
import { ProblemAlert } from "~/components/problem-alert";
import type { ServiceTypeItem } from "~/services/endpoints/service-types";
import type { ActionFailure } from "~/types/action-result";
import type { OrderFormField } from "./order-fields";
import {
  DueDateField,
  OrderDetailsFields,
  ServiceTypeField,
} from "./order-fields";

type OrderFormProps = {
  serviceTypes: ServiceTypeItem[];
  submitLabel: string;
  failure?: ActionFailure<OrderFormField>;
};

export function OrderForm({
  serviceTypes,
  submitLabel,
  failure,
}: OrderFormProps) {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const errors = failure?.fieldErrors ?? {};

  return (
    <Form method="post" className="flex max-w-xl flex-col gap-4">
      {failure && <ProblemAlert failure={failure} />}

      <fieldset className="fieldset flex flex-col gap-3">
        <legend className="fieldset-legend">
          {t("orders.groups.details")}
        </legend>
        <OrderDetailsFields errors={errors} />
      </fieldset>

      <fieldset className="fieldset flex flex-col gap-3">
        <legend className="fieldset-legend">
          {t("orders.groups.schedule")}
        </legend>
        <ServiceTypeField serviceTypes={serviceTypes} errors={errors} />
        <DueDateField errors={errors} />
      </fieldset>

      <div className="flex gap-2">
        <button
          type="submit"
          className="btn btn-primary"
          disabled={navigation.state === "submitting"}
        >
          {submitLabel}
        </button>
        <Link to="/" className="btn btn-ghost">
          {t("common.cancel")}
        </Link>
      </div>
    </Form>
  );
}
