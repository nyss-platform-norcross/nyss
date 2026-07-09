import React from "react";
import { Header } from "./Header";
import { SideMenu } from "./SideMenu";
import { BaseLayout } from "./BaseLayout";
import styles from "./Layout.module.scss";
import { MessagePopup } from "./MessagePopup";
import { Typography, makeStyles, useMediaQuery, useTheme,Grid } from "@material-ui/core";
import { useSelector } from "react-redux";
import useDashboardScrollingTracking from "../../utils/useDashboardScrollingTracking";
import { ProjectMenu } from "./ProjectMenu";
import { TabMenu } from "./TabMenu";
import { BottomMenu } from "./BottomMenu";

const pageContentId = "pageContent";

export const resetPageContentScroll = () => {
  const element = document.getElementById(pageContentId);
  element && element.scrollTo(0, 0);
};

const useStyles = makeStyles((theme) => ({
  mainHeader: {
    textAlign: "center",
    margin: 10
  },
  secondaryHeader: {
    textAlign: "center",
    color: theme.palette.text.secondary
  },
  projectView: {
    paddingBottom: 75
  }
}));

const Layout = ({ fillPage, children }) => {
  const classes = useStyles();
  const handleScroll = useDashboardScrollingTracking();
  const theme = useTheme()
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("sm"));

  const nationalSocietyName = useSelector(
    (state) => state.appData.siteMap.parameters.nationalSocietyName,
  );
  const projectName = useSelector(
    (state) => state.appData.siteMap.parameters.projectName,
  );

  const projectMenu = useSelector(
    (state) => state.appData.siteMap.projectTabMenu
  );

  const isInProjectView = projectMenu.length > 0;

  const title = useSelector(
    (state) => state.appData.siteMap.parameters.title,
  );
  const subTitle = useSelector(
    (state) => state.appData.siteMap.parameters.subTitle,
  );

  return (
    <BaseLayout>
      <SideMenu />
      <div className={styles.mainContent}>
        <Header />
        <div
          className={`
            ${styles.pageContentContainer}
            ${fillPage ? styles.fillPage : null}
            ${isSmallScreen && isInProjectView ? classes.projectView : null}
          `}
          id={pageContentId}
          onScroll={handleScroll}
        >
          <div
            className={`${styles.pageContent} ${
              fillPage ? styles.fillPage : null
            }`}
          >
            <div className={fillPage ? styles.fillPage : null}>
              {nationalSocietyName && !projectName && (
                <Typography variant="h1" className={classes.mainHeader}>
                  {nationalSocietyName}
                </Typography>
              )}
              {projectName && (
                <Grid container direction="column">
                  <Typography variant="body1" className={classes.secondaryHeader}>
                    {nationalSocietyName}
                  </Typography>
                  <Typography variant="h1" className={classes.mainHeader}>
                    {projectName}
                  </Typography>
                </Grid>
              )}
              {!isSmallScreen && <ProjectMenu/>}
              {/* Display tabmenu for all pages except alert assesment page */}
              {(!title && !subTitle) && <TabMenu/>}
              {children}
            </div>
          </div>
        </div>
      </div>
      {isSmallScreen && <BottomMenu/>}
      <MessagePopup />
    </BaseLayout>
  );
};

export default Layout;
