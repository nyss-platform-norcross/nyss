import styles from "../../alerts/components/AlertsAssessmentReport.module.scss";

import {
  Grid,
  Chip,
  Icon,
  Typography,
  useTheme,
  useMediaQuery,
} from "@material-ui/core";
import { makeStyles } from "@material-ui/core/styles";
import { stringKeys, strings } from "../../../strings";

const getReportIcon = (status, rtl) => {
  switch (status) {
    case "Pending":
      return (
        <Icon
          fontSize="small"
          className={`${styles.indicator} ${rtl ? styles.rtl : ""}`}
        >
          hourglass_empty
        </Icon>
      );
    case "New":
      return (
        <Icon
          fontSize="small"
          className={`${styles.indicator} ${rtl ? styles.rtl : ""}`}
        >
          hourglass_empty
        </Icon>
      );
    case "Accepted":
      return (
        <Icon
          fontSize="small"
          className={`${styles.indicator} ${rtl ? styles.rtl : ""}`}
        >
          check
        </Icon>
      );
    case "Rejected":
      return (
        <Icon
          fontSize="small"
          className={`${styles.indicator} ${rtl ? styles.rtl : ""}`}
        >
          clear
        </Icon>
      );
    case "Closed":
      return (
        <Icon
          fontSize="small"
          className={`${styles.indicator} ${rtl ? styles.rtl : ""}`}
        >
          block
        </Icon>
      );
    default:
      return (
        <Icon
          fontSize="small"
          className={`${styles.indicator} ${rtl ? styles.rtl : ""}`}
        >
          warning
        </Icon>
      );
  }
};

const useStyles = makeStyles(() => ({
  Pending: {
    backgroundColor: "#FFE497",
  },
  New: {
    backgroundColor: "#FFE497",
  },
  Accepted: {
    backgroundColor: "#D6F9D5",
  },
  Rejected: {
    backgroundColor: "#E3E3E3",
  },
  Closed: {
    backgroundColor: "#E3E3E3",
  },
  smallStatus: {
    fontSize: "0.75rem",
    whiteSpace: "normal",
  },
}));

export const ReportStatusChip = ({ report, rtl }) => {
  const classes = useStyles();
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("sm"));
  return (
    <Chip
      label={
        <Grid
          container
          justifyContent="center"
          alignItems="center"
          wrap="nowrap"
        >
          <Grid
            container
            alignItems="center"
            item
            style={{
              margin: rtl ? "0 0 0 5px" : "0 5px 0 0",
              width: "fit-content",
            }}
          >
            {getReportIcon(report.status, rtl)}
          </Grid>
          <Grid item>
            <Typography
              variant="body2"
              className={isSmallScreen ? classes.smallStatus : ""}
            >
              {strings(stringKeys.reports.status[report.status])}
            </Typography>
          </Grid>
        </Grid>
      }
      className={`${classes[report.status]}`}
    />
  );
};
