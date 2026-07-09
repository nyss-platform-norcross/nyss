import VisibilityOffOutlinedIcon from "@material-ui/icons/VisibilityOffOutlined";

export function renderDataCollectorDisplayName(row) {
  const dataRemoveText = "--Data removed--";

  if (
    row.dataCollectorDisplayName === dataRemoveText &&
    row.phoneNumber === dataRemoveText
  ) {
    return <VisibilityOffOutlinedIcon />;
  }

  return (
    <>
      {row.dataCollectorDisplayName}
      {!row.isAnonymized && row.dataCollectorDisplayName ? <br /> : ""}
      {(!row.isAnonymized || !row.dataCollector) && row.phoneNumber}
    </>
  );
}

export const renderReportValue = (value, displayZero = false) => {
  return value ? value : (displayZero ? 0 : "");
};
