import { Grid } from "@material-ui/core";
import { TabDropdown } from "./TabDropdown";
import { push } from "connected-react-router";
import { connect } from "react-redux";

const ProjectMenuComponent = ({ projectTabMenu, push }) => {
  const onItemClick = (item) => {
    push(item.url);
  };

  /* Only display project menu for all users other than data consumer since the role only has acces to project dashboard */
  return (
    projectTabMenu.length > 1 && (
      <Grid container justifyContent="center" style={{ marginBottom: 50 }}>
        {projectTabMenu.map((item) => (
          <Grid
            key={`projectTabMenu_${item.url}`}
            item
            style={{ backgroundColor: "#FCFCFC" }}
          >
            <TabDropdown projectTabMenuPage={item} onItemClick={onItemClick} />
          </Grid>
        ))}
      </Grid>
    )
  );
};

const mapStateToProps = (state) => ({
  projectTabMenu: state.appData.siteMap.projectTabMenu,
});

const mapDispatchToProps = {
  push: push,
};

export const ProjectMenu = connect(
  mapStateToProps,
  mapDispatchToProps,
)(ProjectMenuComponent);
