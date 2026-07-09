import React from "react";
import { connect } from "react-redux";
import PropTypes from "prop-types";

export const withAccessRestriction =
  (Component) =>
  ({ roles, condition, ...props }) =>
    roles || condition !== undefined ? (
      <HasAccess roles={roles} condition={condition}>
        <Component {...props} />
      </HasAccess>
    ) : (
      <Component {...props} />
    );

export const HasAccessComponent = ({ user, roles, condition, children }) =>
  (!roles || user.roles.some((role) => roles.indexOf(role) > -1)) &&
  (condition === undefined || condition === true) &&
  children;

const mapStateToProps = (state) => ({
  user: state.appData.user,
});

HasAccessComponent.propTypes = {
  user: PropTypes.any,
  roles: PropTypes.array,
  children: PropTypes.node,
};

export const HasAccess = connect(mapStateToProps)(HasAccessComponent);
