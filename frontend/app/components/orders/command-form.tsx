import { useTranslation } from "react-i18next";
import { useFetcher } from "react-router";
import { isActionFailure } from "~/types/action-result";
import type { OrderIntent } from "~/types/intents/orders";
import type { OrderFieldErrors } from "./order-fields";

type CommandFormProps = {
  intent: OrderIntent;
  legend: string;
  submitLabel: string;
  children: (errors: OrderFieldErrors) => React.ReactNode;
};

export function CommandForm({
  intent,
  legend,
  submitLabel,
  children,
}: CommandFormProps) {
  const { t } = useTranslation();
  const fetcher = useFetcher();
  const failure =
    fetcher.data && isActionFailure(fetcher.data) ? fetcher.data : undefined;

  return (
    <fetcher.Form method="post" className="flex flex-col gap-3">
      <fieldset className="fieldset flex flex-col gap-3">
        <legend className="fieldset-legend">{legend}</legend>
        <input type="hidden" name="intent" value={intent} />

        {failure && (
          <p role="alert" className="alert alert-error">
            {failure.detail ?? failure.title}
          </p>
        )}

        {children(failure?.fieldErrors ?? {})}

        <button
          type="submit"
          className="btn btn-primary self-start"
          disabled={fetcher.state !== "idle"}
        >
          {fetcher.state === "idle" ? submitLabel : t("common.saving")}
        </button>
      </fieldset>
    </fetcher.Form>
  );
}
