import React from "react";

export const wrapLayout = (Layout, Component) => (props) =>
    <Layout><Component {...props} /></Layout>;
