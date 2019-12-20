import React from "react";

export const useLayout = (Layout, Component, fillPage) => (props) =>
  <Layout fillPage={fillPage}><Component {...props} /></Layout>;
