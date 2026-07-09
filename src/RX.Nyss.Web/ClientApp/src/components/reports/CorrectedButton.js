import { IconButton } from "@material-ui/core";
import CheckBox from "@material-ui/icons/CheckBox";
import CheckBoxOutlineBlank from "@material-ui/icons/CheckBoxOutlineBlank";
import { strings, stringKeys } from "../../strings";

const CorrectedButton = ({ isCorrected, onClick }) => {
  const title = isCorrected
    ? strings(stringKeys.reports.list.corrected)
    : strings(stringKeys.reports.list.markAsCorrected);

  return (
    <IconButton color="primary" onClick={onClick} title={title}>
      {isCorrected && <CheckBox color="primary" />}
      {!isCorrected && <CheckBoxOutlineBlank />}
    </IconButton>
  );
};

export default CorrectedButton;
