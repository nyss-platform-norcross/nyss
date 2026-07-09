import styles from "./DataCollectorsPerformanceTableLegend.module.scss";
import { getIconFromStatus } from "../logic/dataCollectorsService";
import { performanceStatus } from "../logic/dataCollectorsConstants";
import { strings, stringKeys } from "../../../strings";
import { DataCollectorStatusIcon } from "../../common/icon/DataCollectorStatusIcon";

export const DataCollectorsPerformanceTableLegend = ({ rtl }) => (
  <div className={styles.legend}>
    <DataCollectorStatusIcon
      status={performanceStatus.reportingCorrectly}
      icon={getIconFromStatus(performanceStatus.reportingCorrectly)}
      rtl={rtl}
    />
    <span className={`${styles.label} ${rtl ? styles.rtl : ""}`}>
      {strings(
        stringKeys.dataCollectors.performanceList.legend.ReportingCorrectly,
      )}
    </span>
    <DataCollectorStatusIcon
      status={performanceStatus.reportingWithErrors}
      icon={getIconFromStatus(performanceStatus.reportingWithErrors)}
      rtl={rtl}
    />
    <span className={`${styles.label} ${rtl ? styles.rtl : ""}`}>
      {strings(
        stringKeys.dataCollectors.performanceList.legend.ReportingWithErrors,
      )}
    </span>
    <DataCollectorStatusIcon
      status={performanceStatus.notReporting}
      icon={getIconFromStatus(performanceStatus.notReporting)}
      rtl={rtl}
    />
    <span>
      {strings(stringKeys.dataCollectors.performanceList.legend.NotReporting)}
    </span>
  </div>
);
