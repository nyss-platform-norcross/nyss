import React from "react";
import { connect } from 'react-redux';
import PropTypes from "prop-types";

export const useAccessRestriction = (Component) => ({ roles, ...props }) =>
  roles
    ? <HasAccess roles={roles}><Component {...props} /></HasAccess>
    : <Component {...props} />;

export const HasAccessComponent = ({ user, roles, children }) =>
  user.roles.some(role => roles.indexOf(role) > -1) && children;

const mapStateToProps = state => ({
  user: state.appData.user
});

HasAccessComponent.propTypes = {
  user: PropTypes.any,
  roles: PropTypes.array,
  children: PropTypes.node
};

export const HasAccess = connect(mapStateToProps)(HasAccessComponent);
