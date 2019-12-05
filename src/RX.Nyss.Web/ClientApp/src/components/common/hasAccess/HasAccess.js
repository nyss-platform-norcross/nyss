import React, { Fragment } from "react";
import { connect } from 'react-redux';
import PropTypes from "prop-types";

export const HasAccessComponent = ({ user, roles, children }) => {

  const hasAccess = () => {
    var access = false;
    user.roles.forEach(role => {
      if (roles.indexOf(role) > -1) {
        access = true;
      }
    });

    return access;
  }

  return hasAccess() ? (
    <Fragment>
      {children}
    </Fragment>
  ) : null;
};

const mapStateToProps = state => ({
  user: state.appData.user
});

HasAccessComponent.propTypes = {
  user: PropTypes.any,
  roles: PropTypes.array,
  children: PropTypes.node
};

export const HasAccess = connect(mapStateToProps)(HasAccessComponent);
