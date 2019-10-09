import { createMuiTheme } from "@material-ui/core/styles";

export const theme = createMuiTheme({
    typography: {
        fontFamily: ["Poppins", '"Helvetica Neue"', 'Arial'].join(','),
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
                marginBottom: 35
            }
        },
        MuiInput: {
            root: {
                border: "1px solid #E4E1E0",
                padding: "5px 10px",
                backgroundColor: "#fff"
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
                fontSize: 16,
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
                fontSize: 16,
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
