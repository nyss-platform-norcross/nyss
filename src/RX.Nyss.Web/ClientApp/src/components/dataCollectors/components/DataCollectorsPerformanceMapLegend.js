import styles from "./DataCollectorsPerformanceMapLegend.module.scss";
import mapStyles from "./DataCollectorsPerformanceMap.module.scss";
import mapIconStyles from "../../common/map/MarkerIcon.module.scss";
import { Icon } from "@material-ui/core";
import { getIconFromStatus } from "../logic/dataCollectorsService";
import { performanceStatus } from "../logic/dataCollectorsConstants";
import { strings, stringKeys } from "../../../strings";

export const DataCollectorsPerformanceMapLegend = ({ rtl }) => {
  const renderItem = (status) => (
    <div className={styles.item}>
      <div
        className={`${mapIconStyles.marker} ${mapStyles[`marker_${status}`]} ${
          styles.icon
        } ${rtl ? styles.rtl : ""}`}
      >
        <Icon>{getIconFromStatus(status)}</Icon>
      </div>
      <div className={styles.description}>
        {strings(stringKeys.dataCollectors.mapOverview.legend[status])}
      </div>
    </div>
  );

  return (
    <div className={styles.legend}>
      {renderItem(performanceStatus.reportingCorrectly)}
      {renderItem(performanceStatus.reportingWithErrors)}
      {renderItem(performanceStatus.notReporting)}
    </div>
  );
};
