import { makeStyles } from "@material-ui/core/styles";
import { SubMenuTitle } from "../../layout/SubMenuTitle";

const useStyles = makeStyles(() => ({
  tableHeader: {
    display: "flex",
    flexWrap: "wrap",
    justifyContent: "space-between",
    alignItems: "center",
    "@media screen and (max-width: 500px)": {
      flexDirection: "column",
    },
  },
}));

const TableHeader = ({ children }) => {
  const classes = useStyles();

  return (
    <div className={classes.tableHeader}>
      <SubMenuTitle />
      {children}
    </div>
  );
};

export default TableHeader;
