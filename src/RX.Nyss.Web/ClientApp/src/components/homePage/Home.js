import React, { useEffect } from 'react';
import { Layout } from '../layout/Layout';
import { useLayout } from '../../utils/layout';
import Typography from '@material-ui/core/Typography';
import { push } from "connected-react-router";
import { connect } from "react-redux";

const HomeComponent = ({ push }) => {
  useEffect(() => { push("/nationalsocieties") });

  return (
    <div>
      <Typography variant="h2">Dashboard</Typography>
    </div>
  );
}


const mapStateToProps = state => ({
});

const mapDispatchToProps = {
  push: push
};

export const Home = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(HomeComponent)
);
