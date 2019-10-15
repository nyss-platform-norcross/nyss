import React from "react";

export const useLayout = (Layout, Component) => (props) =>
    <Layout><Component {...props} /></Layout>;
