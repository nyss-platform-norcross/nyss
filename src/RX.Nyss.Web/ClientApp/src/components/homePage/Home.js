import React, { useEffect } from "react";
import Layout from "../layout/Layout";
import { withLayout } from "../../utils/layout";
import { Typography } from "@material-ui/core";
import { push } from "connected-react-router";
import { connect } from "react-redux";

const HomeComponent = ({ user, push }) => {
  useEffect(() => {
    if (!user.homePage) {
      return;
    }
    push(getHomePageUrl(user));
  }, [user, push]);

  return (
    <div>
      <Typography variant="h2">Dashboard</Typography>
    </div>
  );
};

const getHomePageUrl = (user) => {
  switch (user.homePage.page) {
    case "Root":
      return "/nationalsocieties";
    case "NationalSociety":
      return `/nationalSocieties/${user.homePage.nationalSocietyId}`;
    case "Project":
      return `/nationalsocieties/${user.homePage.nationalSocietyId}/projects/${user.homePage.projectId}`;
    case "ProjectList":
      return `/nationalSocieties/${user.homePage.nationalSocietyId}/projects`;
    default:
      return "/";
  }
};

const mapStateToProps = (state) => ({
  user: state.appData.user,
});

const mapDispatchToProps = {
  push: push,
};

export const Home = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HomeComponent),
);
