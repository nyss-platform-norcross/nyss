import React from 'react';
import styles from './Breadcrumb.module.scss';
import Breadcrumbs from '@material-ui/core/Breadcrumbs';
import NavigateNextIcon from '@material-ui/icons/NavigateNext';

function renderItem(title, label, isSelected) {
    return (
        <div className={`${styles.item} ${isSelected ? styles.selected : ""}`}>
            <div className={styles.title}>{title}</div>
            <div className={styles.label}>{label}</div>
        </div>
    );
}

export const Breadcrumb = () => {
    return (
        <div>
            <Breadcrumbs className={styles.container} separator={<NavigateNextIcon fontSize="small" />} aria-label="breadcrumb">
                {renderItem("Overview", "All national societies")}
                {renderItem("Sierra Leone", "National society", true)}
            </Breadcrumbs>
        </div>
    );
}
