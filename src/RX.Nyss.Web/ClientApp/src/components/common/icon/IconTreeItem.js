import styles from './StyledTreeItem.module.scss'
import React from 'react';
import PropTypes from 'prop-types';
import TreeItem from '@material-ui/lab/TreeItem';
import Typography from '@material-ui/core/Typography';


export function StyledTreeItem(props) {
  const { name, labelText, labelIcon: LabelIcon, ...other} = props;

  return (
    <TreeItem
      label={
        <div className={styles.labelRoot}>
          <LabelIcon className={styles['icon_' + name]} />
          <Typography >
            {labelText}
          </Typography>
        </div>
      }
    />
  );
}

StyledTreeItem.propTypes = {
  labelIcon: PropTypes.elementType.isRequired,
};

