import React, { useEffect } from 'react';
import { Layout } from '../layout/Layout';
import { useLayout } from '../../utils/layout';
import Typography from '@material-ui/core/Typography';
import { push } from "connected-react-router";
import { connect } from "react-redux";

const HomeComponent = ({ user, push }) => {
  useEffect(() => {
    if (!user.homePage){
      return;
    }
    push(getHomePageUrl(user))
  }, [user, push]);

  return (
    <div>
      <Typography variant="h2">Dashboard</Typography>
    </div>
  );
}

const getHomePageUrl = (user) => {
  switch (user.homePage.page) {
    case "Root":
        return "/nationalsocieties";
    case "NationalSociety":
        return `/nationalSocieties/${user.homePage.nationalSocietyId}`;
    case "Project":
        return `/api/project/${user.homePage.projectId}`;
    default:
        return '/';
  }
};

const mapStateToProps = state => ({
  user: state.appData.user
});

const mapDispatchToProps = {
  push: push
};

export const Home = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HomeComponent)
);
