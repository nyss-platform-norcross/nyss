import { List, ListItem, ListItemText, ListItemIcon, ListSubheader, Divider, makeStyles } from "@material-ui/core";
import { RcIcon } from '../icons/RcIcon';

const useStyles = makeStyles(() => ({
  SideMenuIcon: {
    fontSize: '26px',
    color: '#1E1E1E',
  },
  ListItemIconWrapper: {
    minWidth: '20px',
  },
  SideMenuText: {
    color: '#1E1E1E',
    fontSize: '16px',
  },
  SideMenuTextWrapper: {
    padding: '16px 12px 16px 12px',
  },
  ListItemActive: {
    backgroundColor: '#E3E3E3',
    "& span": {
      fontWeight: '600',
    },
  },
  ListItem : {
    padding: '0 0 0 24px',
  },
  SubHeader : {
    color: '#1E1E1E',
    lineHeight: '28px',
    fontSize: 12,
    fontWeight: "bold",
    margin: "0 0 0px 8px"
  },
  Divider: {
    backgroundColor: '#B4B4B4',
  },

}));

const mapPathToSideMenuIcon = (path) => {
  if (path.includes('dashboard')) {
    return "Dashboard"
  } else if (path.includes('reports')) {
    return "Report"
  } else if (path.includes('users')) {
    return "Users"
  } else if (path.includes('settings')) {
    return "Settings"
  } else if (path.includes('datacollectors')) {
    return "DataCollectors"
  } else if (path.includes('alerts')) {
    return "Alerts"
  } else if (path.includes('overview')) {
    return "Settings"
  } else if (path.includes('globalcoordinators')) {
    return "GlobalCoordinators"
  } else if (path.includes('healthrisks')) {
    return "HealthRisks"
  } else if (path.includes('projects')) {
    return "Project"
  } else if (path.includes('nationalsocieties')) {
    return "NationalSocieties"
  } else {
    return "Dashboard"
  }
}

export const MenuSection = ({menuItems, handleItemClick, menuTitle}) => {
  const classes = useStyles();


  return(
    <List component="nav" aria-label={`${menuTitle} navigation menu`}
      subheader={
      <>
        <ListSubheader component="div" id={menuTitle} className={classes.SubHeader} disableGutters>
        {menuTitle}
      </ListSubheader>
      <Divider className={classes.Divider}/>
      </>
    }>
    {menuItems.map((item) => {
      return (
        <ListItem key={`sideMenuItem_${item.title}`} className={`${classes.ListItem} ${item.isActive ? classes.ListItemActive : ''}`} button onClick={() => handleItemClick(item)} >
          <ListItemIcon className={classes.ListItemIconWrapper}>
            {item.url && <RcIcon icon={mapPathToSideMenuIcon(item.url)} className={`${classes.SideMenuIcon} `} />}
          </ListItemIcon>
          <ListItemText disablePadding primary={item.title} primaryTypographyProps={{ 'className': classes.SideMenuText }} className={classes.SideMenuTextWrapper}/>
        </ListItem>
      )
    })}
  </List>
  )
}