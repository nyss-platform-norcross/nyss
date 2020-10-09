import { createMuiTheme } from "@material-ui/core/styles";

export const theme = createMuiTheme({
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
      dark: "#3d5afe",
      contrastText: "#ffffff"
    },
    secondary: {
      light: "#8c9eff",
      main: "#333333",
      dark: "#3d5afe",
      contrastText: "#ffffff"
    }
  },
  overrides: {
    MuiButton: {
      root: {
        borderRadius: 0,
        padding: "10px 15px",
        textTransform: "none",
        fontSize: 16
      },
      outlinedPrimary: {
        border: "2px solid #C02C2C !important"
      },
      text: {
        textDecoration: "underline"
      },
      startIcon: {
        "@media (max-width: 1279px)": {
          display: "none"
        }
      }
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
        background: '#FAFAFA',
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
        textOverflow: "ellipsis"
      },
      primary: {
        fontSize: 16,
        color: "#737373"
      }
    },
    MuiTableCell: {
      root: {
        fontSize: "1rem"
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
        cursor: "pointer"
      },
      iconContainer: {
        cursor: "pointer",
        "&:empty": {
          cursor: "default"
        }
      },
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
        paddingRight: 16
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
    }
  },
});
