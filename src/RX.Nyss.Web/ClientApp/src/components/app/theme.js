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
      fontSize: "24px",
      fontWeight: "bold",
      marginBottom: 50
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
        border: "2px solid #C02C2C !important",
        padding: "10px 15px"
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
    MuiFormControl: {
      root: {
        marginTop: 20,
        marginBottom: 20
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
    MuiInputLabel: {
      root: {
        padding: "5px 10px",
        zIndex: 1,
        fontWeight: 600
      },
      shrink: {
        padding: 0,
        color: "#000000 !important",
        transform: "translate(0, -6px);"
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
          backgroundColor: "#fff"
        },
      }
    },
    MuiListItemText: {
      root: {
        padding: "8px 20px"
      }
    }
  },
});
