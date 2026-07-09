import styles from "./LocationFilter.module.scss";
import ExpandMoreIcon from "@material-ui/icons/ExpandMore";
import ExpandLessIcon from "@material-ui/icons/ExpandLess";
import ErrorOutlineIcon from "@material-ui/icons/ErrorOutline";
import { Fragment, useEffect, useState } from "react";
import { Checkbox } from "@material-ui/core";

const LocationItem = ({ type, data, isVisible, onChange, rtl }) => {
  const [isExpanded, setIsExpanded] = useState(false);
  const [childData, setChildData] = useState(null);

  useEffect(() => {
    const getChildData = () => {
      switch (type) {
        case "region":
          return { type: "district", data: data.districts };
        case "district":
          return { type: "village", data: data.villages };
        case "village":
          return { type: "zone", data: data.zones };
        default:
          return { type: "", data: [] };
      }
    };

    setChildData(getChildData());
  }, [data, type]);

  const handleChange = (id) => {
    onChange({ type: type, id: id });
  };

  const renderIconContainer = () => {
    if (type === "unknown") {
      return (
        <span className={styles.iconContainer}>
          <ErrorOutlineIcon color="primary" />
          <Checkbox
            className={styles.clickable}
            checked={data.selected}
            color="primary"
            onClick={() => handleChange(data.id)}
          />
        </span>
      );
    }

    if (!!childData && childData.data.length < 1) {
      return (
        <span
          className={`${styles.iconContainer} ${rtl ? styles.rtl : ""}`}
          nodata="true"
        >
          <Checkbox
            className={styles.clickable}
            checked={data.selected}
            color="primary"
            onClick={() => handleChange(data.id)}
          />
        </span>
      );
    }

    return (
      <span className={styles.iconContainer}>
        {isExpanded ? (
          <ExpandLessIcon
            className={styles.clickable}
            onClick={() => setIsExpanded(false)}
          />
        ) : (
          <ExpandMoreIcon
            className={styles.clickable}
            onClick={() => setIsExpanded(true)}
          />
        )}
        <Checkbox
          className={styles.clickable}
          checked={data.selected}
          color="primary"
          onClick={() => handleChange(data.id)}
        />
      </span>
    );
  };

  return (
    isVisible && (
      <Fragment>
        <div
          className={`${styles.locationFilterItem} ${rtl ? styles.rtl : ""}`}
          type={type}
        >
          {renderIconContainer()}

          <span
            className={styles.clickable}
            onClick={() => handleChange(data.id)}
          >
            {data.name}
          </span>
        </div>

        {!!childData &&
          childData.data.map((d) => (
            <LocationItem
              key={`${childData.type}_${d.id}`}
              type={childData.type}
              data={d}
              isVisible={isExpanded}
              onChange={onChange}
              rtl={rtl}
            />
          ))}
      </Fragment>
    )
  );
};

export default LocationItem;
