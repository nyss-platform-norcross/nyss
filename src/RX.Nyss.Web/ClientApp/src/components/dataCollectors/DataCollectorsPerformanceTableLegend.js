import styles from "./DataCollectorsPerformanceTableLegend.module.scss";

import React from 'react';
import { DataCollectorStatusIcon } from '../common/icon/DataCollectorStatusIcon';
import { getIconFromStatus } from "./logic/dataCollectorsService";
import { performanceStatus } from "./logic/dataCollectorsConstants";
import { strings, stringKeys } from "../../strings";

export const DataCollectorsPerformanceTableLegend = () => {
  const renderItem = (status) => (
    <div className={styles.item}>
      <DataCollectorStatusIcon status={status} icon={getIconFromStatus(status)} />
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
