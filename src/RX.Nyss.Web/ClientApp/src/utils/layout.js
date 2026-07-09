import React from "react";

export const withLayout = (Layout, Component, fillPage) => (props) => (
  <Layout fillPage={fillPage}>
    <Component {...props} />
  </Layout>
);
