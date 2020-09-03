import styles from "./DataCollectorsPerformanceTableLegend.module.scss";
import React from 'react';
import { getIconFromStatus } from "./logic/dataCollectorsService";
import { performanceStatus } from "./logic/dataCollectorsConstants";
import { strings, stringKeys } from "../../strings";
import { DataCollectorStatusIcon } from "../common/icon/DataCollectorStatusIcon";

export const DataCollectorsPerformanceTableLegend = () => (
  <div className={styles.legend}>
    <DataCollectorStatusIcon status={performanceStatus.reportingCorrectly} icon={getIconFromStatus(performanceStatus.reportingCorrectly)} />
    <span className={styles.label}>{strings(stringKeys.dataCollector.performanceList.legend.ReportingCorrectly)}</span>
    <DataCollectorStatusIcon status={performanceStatus.reportingWithErrors} icon={getIconFromStatus(performanceStatus.reportingWithErrors)} />
    <span className={styles.label}>{strings(stringKeys.dataCollector.performanceList.legend.ReportingWithErrors)}</span>
    <DataCollectorStatusIcon status={performanceStatus.notReporting} icon={getIconFromStatus(performanceStatus.notReporting)} />
    <span>{strings(stringKeys.dataCollector.performanceList.legend.NotReporting)}</span>
  </div>
);
