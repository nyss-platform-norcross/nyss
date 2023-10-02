import { createTheme } from "@material-ui/core/styles";

export const theme = (direction) => createTheme({
  direction: direction,
  typography: {
    fontSize: 14,
    fontFamily: ["Poppins", '"Helvetica Neue"', 'Arial'].join(','),
    body1: {
      color: "#333333"
    },
    button: {
      color: "#333333"
    },
    h1: {
      fontSize: "48px",
      color: "#C02C2C",
      fontWeight: "400",
      marginBottom: 20
    },
    h2: {
      fontSize: 22,
      fontWeight: 600,
      margin: "10px 0 30px"
    },
    h3: {
      fontSize: 16,
      fontWeight: 600,
      margin: "10px 0 20px"
    },
    h6: {
      fontSize: 14,
      fontWeight: 600,
      margin: "10px 0 10px"
    }
  },
  palette: {
    primary: {
      light: "#8c9eff",
      main: "#C02C2C",
      dark: "#DC3F2E",
      contrastText: "#ffffff"
    },
    secondary: {
      light: "#8c9eff",
      main: "#333333",
      dark: "#3d5afe",
      contrastText: "#ffffff"
    },
  },
  overrides: {
    MuiButton: {
      root: {
        padding: "7px 15px",
        textTransform: "none",
        fontSize: 16
      },
      outlinedPrimary: {
        border: "2px solid #C02C2C !important"
      },
    },
    MuiMenu: {
      paper: {
        maxHeight: 200
      }
    },
    MuiBreadcrumbs: {
      root: {
        fontSize: 16
      },
      separator: {
        paddingBottom: 2
      }
    },
    MuiPaper: {
      root: {
        border: "none",
      },
      elevation1: {
        boxShadow: "none",
        borderRadius: 0,
      }
    },
    MuiInput: {
      root: {
        fontSize: "1rem",
        border: "1px solid #E4E1E0",
        backgroundColor: "#fff"
      },
      input: {
        padding: "10px",
        '&:-webkit-autofill': {
          transitionDelay: '9999s'
        },
        "&$disabled": {
          backgroundColor: "#FAFAFA"
        },
      },
      formControl: {
        marginTop: "30px !important"
      },
      multiline: {
        padding: "10px"
      },
      inputMultiline: {
        padding: "0"
      },
      underline: {
        "&:before": {
          borderBottom: "none !important"
        },
        "&:after": {
          borderBottomColor: "#a0a0a0"
        },
        "&:hover": {
        }
      }

    },
    MuiFormLabel: {
      root: {
        color: "#333333 !important",
        transform: "translate(0, 2px);",
        lineHeight: "inherit"
      },
      focused: {},
    },
    MuiFormControlLabel: {
      root: {
        marginRight: direction === 'ltr' ? '16px' : '-11px',
        marginLeft: direction === 'ltr' ? '-11px' : '16px'
      }
    },
    MuiInputLabel: {
      root: {
        zIndex: 1
      },
      shrink: {
        transform: "translate(0, 2px);",
        right: 0,
        lineHeight: "inherit",
        overflow: "hidden",
        textOverflow: "ellipsis"
      }
    },
    MuiInputAdornment: {
      positionEnd: {
        marginLeft: '3px'
      }
    },
    MuiLink: {
      underlineHover: {
        textDecoration: "underline"
      }
    },
    MuiListItem: {
      root: {
        paddingLeft: 20,
        "&$selected": {
          backgroundColor: "rgba(0, 0, 0, 0.06)",
        },
      }
    },
    MuiListItemText: {
      root: {
        padding: "12px 20px",
        overflow: "hidden",
        textOverflow: "ellipsis",
        textAlign: direction === 'ltr' ? 'left' : 'right'
      },
      primary: {
        fontSize: 16,
        color: "#737373"
      }
    },
    MuiTable: {
      root: {
        borderTop: "2px solid #f3f3f3",
        borderLeft: "2px solid #f3f3f3",
        borderRight: "2px solid #f3f3f3",
      }
    },
    MuiTableCell: {
      root: {
        fontSize: "1rem",
        textAlign: direction === 'ltr' ? 'left' : 'right',
        background: "#FFFFFF",
      },
      head: {
        fontWeight: 600,
        borderBottomColor: "#8C8C8C",
      }
    },
    MuiTabs: {
      root: {
        borderBottom: "1px solid #e0e0e0",

      },
      indicator: {
        backgroundColor: "#C02C2C",
        height: 3
      }
    },
    MuiCard: {
      root: {
        border: "2px solid #f3f3f3"
      }
    },
    MuiCardHeader: {
      title: {
        fontSize: "16px",
        fontWeight: "bold"
      }
    },
    MuiTab: {
      root: {
        fontSize: "1rem !important",
        "&:hover": {
          backgroundColor: "rgba(0, 0, 0, 0.04)",
          opacity: 1
        },
      },
      textColorInherit: {
        opacity: 1
      }
    },
    MuiTreeItem: {
      root: {},
      content: {
        padding: "8px",
        cursor: "default !important",
        "&:hover": {
          backgroundColor: "rgba(0, 0, 0, 0.04)"
        }
      },
      selected: {},
      label: {
        backgroundColor: "transparent !important",
        cursor: "pointer",
        paddingLeft: direction === 'ltr' ? '4px' : null,
        paddingRight: direction === 'ltr' ? null : '4px'
      },
      iconContainer: {
        cursor: "pointer",
        "&:empty": {
          cursor: "default"
        }
      },
      group: {
        marginLeft: direction === 'ltr' ? '17px' : null,
        marginRight: direction === 'ltr' ? null : '17px'
      }
    },
    MuiExpansionPanel: {
      root: {
        border: "2px solid #f3f3f3",
        "&$disabled": {
          backgroundColor: "#FAFAFA"
        }
      }
    },
    MuiExpansionPanelActions: {
      root: {
        padding: "15px 20px"
      }
    },
    MuiDivider: {
      root: {
        backgroundColor: "#f3f3f3"
      }
    },
    MuiDialogTitle: {
      root: {
        paddingBottom: 0
      }
    },
    MuiRadio: {
      colorSecondary: {
        '&$checked': {
          color: '#C02C2C'
        }
      }
    },
    MuiTableSortLabel: {
      root: {
        verticalAlign: "unset"
      }
    },
    MuiAutocomplete: {
      input: {
        padding: '10px !important'
      },
      inputRoot: {
        paddingRight: direction === 'ltr' ? 56 : '0px !important',
        paddingLeft: direction === 'ltr' ? null : 56
      },
      endAdornment: {
        right: direction === 'ltr' ? 0 : null,
        left: direction === 'ltr' ? null : 0
      }
    },
    MuiTooltip: {
      tooltip: {
        fontSize: '1rem',
        padding: '8px'
      }
    },
    MuiSwitch: {
      root: {
        marginTop: '7px !important'
      }
    },
    MuiLinearProgress: {
      root: {
        marginBottom: "-3px",
        height: "3px"
      }
    },
    MuiPopover: {
      paper: {
        maxWidth: '400px'
      }
    },
    MuiSelect: {
      icon: {
        left: direction === 'ltr' ? null : 0,
        right: direction === 'ltr' ? 0 : null
      },
      select: {
        paddingRight: direction === 'ltr' ? '24px' : '10px !important',
        paddingLeft: direction === 'ltr' ? 10 : '24px'
      }
    },
    MuiChip: {
      deleteIcon: {
        margin: direction === 'ltr' ? '0 5px 0 -6px' : '0 -6px 0 5px'
      },
      icon: {
        marginLeft: direction === 'ltr' ? '5px' : '-6px',
        marginRight: direction === 'ltr' ? '-6px' : '5px'
      }
    },
    MuiFormHelperText: {
      root: {
        textAlign: direction === 'ltr' ? 'left' : 'right'
      }
    },
    MuiSnackbarContent: {
      action: {
        marginLeft: direction === 'ltr' ? 'auto' : '-8px',
        marginRight: direction === 'ltr' ? '-8px' : 'auto',
        paddingLeft: direction === 'ltr' ? '16px' : '0',
        paddingRight: direction === 'ltr' ? '0' : '16px'
      }
    }
  },
});
