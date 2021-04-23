import styles from './IconTreeItem.module.scss';
import areaFilterStyles from '../filters/AreaFilter.module.scss';
import React from 'react';
import PropTypes from 'prop-types';
import TreeItem from '@material-ui/lab/TreeItem';
import Typography from '@material-ui/core/Typography';
import {stringKeys, strings} from "../../../strings";

export function IconTreeItem({ nodeId, labelText, labelIcon: LabelIcon, onSelect, isSelected }) {

  const handleChange = (e) => {
    const data = { type: 'unknown', name: strings(stringKeys.filters.area.unknown)};
    e.stopPropagation();
    onSelect(data);
  }

  return (
    <TreeItem
      nodeId={nodeId}
      className={isSelected ? areaFilterStyles.selected : null}
      label={
        <div className={styles.labelRoot} onClick={handleChange}>
          <LabelIcon className={styles.icon} color="primary" />
          <Typography >
            {labelText}
          </Typography>
        </div>
      }
    />
  );
}

IconTreeItem.propTypes = {
  labelIcon: PropTypes.elementType.isRequired,
};

