import React from 'react';
import styles from './Breadcrumb.module.scss';
import Breadcrumbs from '@material-ui/core/Breadcrumbs';
import NavigateNextIcon from '@material-ui/icons/NavigateNext';

const renderItem = (title, label, isSelected) => (
  <div className={`${styles.item} ${isSelected ? styles.selected : ""}`}>
    <div className={styles.title}>{title}</div>
  </div>
);

export const Breadcrumb = () => (
  <Breadcrumbs
    className={styles.container}
    separator={<NavigateNextIcon fontSize="small" color="primary" />}>
    {renderItem("Overview", "All national societies")}
    {renderItem("Sierra Leone", "National society", true)}
  </Breadcrumbs>
);
