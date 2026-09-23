import { useTranslation } from "react-i18next";
import type { ActionFailure } from "~/types/action-result";

export function ProblemAlert({ failure }: { failure: ActionFailure }) {
  const { t } = useTranslation();

  return (
    <p role="alert" className="alert alert-error">
      {t(`problems.${failure.code}`)}
    </p>
  );
}
