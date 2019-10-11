import React from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";

import styles from './Layout.module.scss';
import { Loading } from '../common/loading/Loading';

const BaseLayoutComponent = ({ appReady, children }) => {
    if (!appReady) {
        return (
            <div className={styles.loader}>
                <Loading />
            </div>);
    }

    return (
        <div className={styles.layout}>
            {children}
        </div>
    );
}

const mapStateToProps = state => ({
    appReady: state.appData.appReady
});

BaseLayoutComponent.propTypes = {
    appReady: PropTypes.bool
};

export const BaseLayout = connect(mapStateToProps)(BaseLayoutComponent);
