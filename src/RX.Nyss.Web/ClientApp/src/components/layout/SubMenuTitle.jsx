import { connect } from "react-redux";
import { Typography } from "@material-ui/core";

const SubMenuTitleComponent = ({ title, ...props }) => {
  return (
    <Typography variant="h2" {...props}>
      {title}
    </Typography>
  );
};

const mapStateToProps = (state) => ({
  title: state.appData.siteMap.title,
});

export const SubMenuTitle = connect(mapStateToProps, {})(SubMenuTitleComponent);
