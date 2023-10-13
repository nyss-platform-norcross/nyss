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
  },
  Divider: {
    backgroundColor: '#B4B4B4',
  },

}));

export const MenuSection = ({menuItems, handleItemClick, menuTitle}) => {
  const classes = useStyles();

  return(
    <List component="nav" aria-label={`${menuTitle} navigation menu`}
      subheader={
      <>
        <ListSubheader component="div" id={menuTitle} className={classes.SubHeader}>
        {menuTitle}
      </ListSubheader>
      <Divider className={classes.Divider}/>
      </>
    }>
    {menuItems.map((item) => {
      return (
        <ListItem key={`sideMenuItem_${item.title}`} className={`${classes.ListItem} ${item.isActive ? classes.ListItemActive : ''}`} button onClick={() => handleItemClick(item)} >
          <ListItemIcon className={classes.ListItemIconWrapper}>
            {item.icon && <RcIcon icon={item.icon} className={`${classes.SideMenuIcon} ${item.isActive ? classes.SideMenuIconActive : ''}`} />}
          </ListItemIcon>
          <ListItemText primary={item.title} primaryTypographyProps={{ 'className': classes.SideMenuText }} className={classes.SideMenuTextWrapper}/>
        </ListItem>
      )
    })}
  </List>
  )
}