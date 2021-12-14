import React, { useEffect } from "react";
import { connect } from "react-redux";
import Layout from '../layout/Layout';
import { withLayout } from '../../utils/layout';

const ProjectErrorMessagesPageComponent = (props) => {
  
  useEffect(() => {
    console.log("Loading data for project");
  }, []);
  
  return (
    <p>Hello from error messages. Project {props.projectId}</p>
  )
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
});

const mapDispatchToProps = {
}

export const ProjectErrorMessagesPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectErrorMessagesPageComponent),
);
