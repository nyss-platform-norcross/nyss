import styles from "./DataCollectorsPerformanceTableLegend.module.scss";

import React from 'react';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import { getMarkerIconFromStatus } from "./logic/dataCollectorsService";
import { performanceStatus } from "./logic/dataCollectorsConstants";
import { strings, stringKeys } from "../../strings";

export const DataCollectorsPerformanceTableLegend = () => {
  const renderItem = (status) => (
    <div className={styles.item}>
      <DataCollectorStatusIcon className={styles.icon} status={status} icon={getMarkerIconFromStatus(status)} />
      <div className={styles.description}>{strings(stringKeys.dataCollector.performanceList.legend[status])}</div>
    </div>
  );

  return (
    <div className={styles.legend}>
      {renderItem(performanceStatus.reportingCorrectly)}
      {renderItem(performanceStatus.reportingWithErrors)}
      {renderItem(performanceStatus.notReporting)}
    </div>
  );
}
