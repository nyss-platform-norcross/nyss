import React from "react";
import {
  IconButton,
  Icon,
} from "@material-ui/core";
import AssignmentTurnedIn from "@material-ui/icons/AssignmentTurnedIn";
import { strings, stringKeys } from '../../strings';

const CorrectedButton = ({ isCorrected, onClick }) => {
  const title = isCorrected
    ? strings(stringKeys.reports.list.corrected)
    : strings(stringKeys.reports.list.markAsCorrected);
  
  return (
    <IconButton color="primary" disabled={isCorrected} onClick={onClick} title={title}>
      {isCorrected && <Icon style={{color: "#15ab15"}}>check</Icon>}
      {!isCorrected && <AssignmentTurnedIn />}
    </IconButton>
  );
}

export default CorrectedButton;
