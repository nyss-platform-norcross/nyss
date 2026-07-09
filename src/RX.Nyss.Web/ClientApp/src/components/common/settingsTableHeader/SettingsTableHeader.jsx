import { makeStyles } from "@material-ui/core/styles";
import { SubMenuTitle } from "../../layout/SubMenuTitle";

const useStyles = makeStyles(() => ({
  settingsHeader: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
  },
}));

const SettingsTableHeader = ({ children }) => {
  const classes = useStyles();

  return (
    <div className={classes.settingsHeader}>
      <SubMenuTitle />
      {children}
    </div>
  );
};

export default SettingsTableHeader;
