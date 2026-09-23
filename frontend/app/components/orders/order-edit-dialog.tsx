import { useTranslation } from "react-i18next";
import { CommandDialog } from "~/components/orders/command-dialog";
import {
  DueDateField,
  OrderDetailsFields,
  type OrderFieldErrors,
  ServiceTypeField,
} from "~/components/orders/order-fields";
import type { MaintenanceOrderResponse } from "~/services/endpoints/orders";
import type { ServiceTypeItem } from "~/services/endpoints/service-types";
import {
  type EditCommand,
  editCommandLabels,
  OrderIntent,
} from "~/types/intents/orders";

export function OrderEditDialog({
  command,
  order,
  serviceTypes,
  onClose,
}: {
  command: EditCommand;
  order: MaintenanceOrderResponse;
  serviceTypes: ServiceTypeItem[];
  onClose: () => void;
}) {
  const { t } = useTranslation();

  return (
    <CommandDialog
      intent={command}
      title={t(editCommandLabels[command])}
      submitLabel={t("common.save")}
      onClose={onClose}
    >
      {(errors) => (
        <>
          <input type="hidden" name="version" value={order.version} />
          {fieldsFor(command, order, serviceTypes, errors)}
        </>
      )}
    </CommandDialog>
  );
}

function fieldsFor(
  command: EditCommand,
  order: MaintenanceOrderResponse,
  serviceTypes: ServiceTypeItem[],
  errors: OrderFieldErrors,
) {
  switch (command) {
    case OrderIntent.CorrectDetails:
      return <OrderDetailsFields defaults={order} errors={errors} />;
    case OrderIntent.ChangeService:
      return (
        <ServiceTypeField
          serviceTypes={serviceTypes}
          defaults={order}
          errors={errors}
        />
      );
    case OrderIntent.Reschedule:
      return <DueDateField defaults={order} errors={errors} />;
  }
}
