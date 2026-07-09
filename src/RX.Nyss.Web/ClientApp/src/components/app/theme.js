import { createTheme } from "@material-ui/core/styles";
const { palette } = createTheme();

export const nyssPalette = {
  primary: {
    main: "#D52B1E",
    contrastText: "#ffffff",
    light: "#FEE7E7",
  },
  secondary: {
    light: "#8c9eff",
    main: "#333333",
    dark: "#3d5afe",
    contrastText: "#ffffff",
  },
  additional1: {
    light: "#CDDDE7",
    main: "#7FA7B7",
    dark: "#355770",
    lightest: "#E5EEF2",
  },
  background: {
    light: "#ffffff",
    default: "#F1F1F1",
  },
  backgroundDark: palette.augmentColor({
    main: "#F4F4F4",
  }),
  text: {
    secondary: "#4F4F4F",
  },
};

export const theme = (direction) =>
  createTheme({
    breakpoints: {
      values: {
        xs: 0,
        sm: 600,
        md: 900,
        lg: 1200,
        xl: 1536,
      },
    },
    direction: direction,
    typography: {
      fontFamily: "MyriadPro, Arial",
      body1: {
        fontSize: "1rem",
        color: "#333333",
      },
      body2: {
        fontSize: "0.875rem",
      },
      button: {
        color: "#333333",
      },
      h1: {
        fontSize: "2rem",
        fontWeight: "400",
        margin: "30px 0 30px",
      },
      h2: {
        fontSize: "1.75rem",
        fontWeight: 600,
        margin: "10px 0 10px",
      },
      h3: {
        fontSize: "1.5rem",
        fontWeight: 600,
        margin: "10px 0 10px",
      },
      h4: {
        fontSize: "1.25rem",
        fontWeight: 600,
        margin: "10px 0 10px",
      },
      h5: {
        fontSize: "1.125rem",
        fontWeight: 600,
        margin: "10px 0 10px",
      },
      h6: {
        fontSize: "1rem",
        fontWeight: 600,
        margin: "10px 0 10px",
      },
    },
    palette: nyssPalette,
    overrides: {
      MuiButton: {
        root: {
          padding: "7px 15px",
          textTransform: "none",
          fontSize: "1rem",
        },
        outlinedPrimary: {
          border: "2px solid #D52B1E !important",
        },
        containedPrimary: {
          boxShadow: "none",
          "&:hover": {
            boxShadow: "none",
          },
        },
      },
      MuiIconButton: {
        root: {
          fontSize: "22px",
          padding: "4px",
        },
      },
      MuiMenu: {
        paper: {
          maxHeight: 200,
        },
      },
      MuiPaper: {
        root: {
          border: "none",
        },
        elevation1: {
          boxShadow: "none",
          borderRadius: 0,
        },
      },
      MuiInput: {
        root: {
          fontSize: "1rem",
          border: "1px solid #E4E1E0",
          backgroundColor: "#fff",
        },
        input: {
          padding: "10px",
          "&:-webkit-autofill": {
            transitionDelay: "9999s",
          },
          "&$disabled": {
            backgroundColor: "#FAFAFA",
          },
        },
        formControl: {
          marginTop: "30px !important",
        },
        multiline: {
          padding: "10px",
        },
        inputMultiline: {
          padding: "0",
        },
        underline: {
          "&:before": {
            borderBottom: "none !important",
          },
          "&:after": {
            borderBottomColor: "#a0a0a0",
          },
          "&:hover": {},
        },
      },
      MuiFormLabel: {
        root: {
          color: "#333333 !important",
          transform: "translate(0, 2px);",
          lineHeight: "inherit",
          fontSize: "1rem",
        },
        focused: {},
      },
      MuiFormControlLabel: {
        root: {
          marginRight: direction === "ltr" ? "0px" : "-4px",
          marginLeft: direction === "ltr" ? "-4px" : "0px",
        },
        label: {
          fontSize: "1rem",
        },
      },
      MuiInputLabel: {
        root: {
          zIndex: 1,
        },
        shrink: {
          transform: "translate(0, 2px);",
          right: 0,
          lineHeight: "inherit",
          overflow: "hidden",
          textOverflow: "ellipsis",
        },
      },
      MuiInputAdornment: {
        positionEnd: {
          marginLeft: "3px",
        },
      },
      MuiLink: {
        underlineHover: {
          textDecoration: "underline",
        },
      },
      MuiListItem: {
        root: {
          paddingLeft: 20,
          "&$selected": {
            backgroundColor: "rgba(0, 0, 0, 0.06)",
          },
        },
      },
      MuiListItemText: {
        root: {
          padding: "12px 20px",
          overflow: "hidden",
          textOverflow: "ellipsis",
          textAlign: direction === "ltr" ? "left" : "right",
        },
        primary: {
          fontSize: "1rem",
          color: "#737373",
        },
      },
      MuiTableContainer: {
        root: {
          overflow: "auto",
          maxHeight: "90vh",
        },
      },
      MuiTable: {
        root: {
          borderTop: "2px solid #f3f3f3",
          borderLeft: "2px solid #f3f3f3",
          borderRight: "2px solid #f3f3f3",
          background: "#FFFFFF",
        },
      },
      MuiTableHead: {
        root: {
          "& .MuiTableCell-root": {
            backgroundColor: nyssPalette.background.light,
          },
          whiteSpace: "normal",
          "@media screen and (max-width: 500px)": {
            whiteSpace: "nowrap",
          }
        },
      },
      MuiTableCell: {
        root: {
          fontSize: "1rem",
          textAlign: direction === "ltr" ? "left" : "right",
        },
        head: {
          fontWeight: 600,
          borderBottomColor: "#8C8C8C",
        },
        stickyHeader: {
          backgroundColor: "#F1F1F1",
        },
      },
      MuiTabs: {
        root: {
          borderBottom: "1px solid #e0e0e0",
        },
        indicator: {
          backgroundColor: "#D52B1E",
          height: 3,
        },
      },
      MuiCard: {
        root: {
          border: "2px solid #f3f3f3",
        },
      },
      MuiCardHeader: {
        title: {
          fontSize: "1rem",
          fontWeight: "bold",
        },
      },
      MuiCardContent: {
        root: {
          "&:last-child": {
            paddingBottom: "16px",
          },
        },
      },
      MuiTab: {
        root: {
          "&:hover": {
            backgroundColor: "rgba(0, 0, 0, 0.04)",
            opacity: 1,
          },
        },
        textColorInherit: {
          opacity: 1,
        },
      },
      MuiTreeItem: {
        root: {},
        content: {
          padding: "8px",
          cursor: "default !important",
          "&:hover": {
            backgroundColor: "rgba(0, 0, 0, 0.04)",
          },
        },
        selected: {},
        label: {
          backgroundColor: "transparent !important",
          cursor: "pointer",
          paddingLeft: direction === "ltr" ? "4px" : null,
          paddingRight: direction === "ltr" ? null : "4px",
        },
        iconContainer: {
          cursor: "pointer",
          "&:empty": {
            cursor: "default",
          },
        },
        group: {
          marginLeft: direction === "ltr" ? "17px" : null,
          marginRight: direction === "ltr" ? null : "17px",
        },
      },
      MuiExpansionPanel: {
        root: {
          border: "2px solid #f3f3f3",
          "&$disabled": {
            backgroundColor: "#FAFAFA",
          },
        },
      },
      MuiExpansionPanelActions: {
        root: {
          padding: "15px 20px",
        },
      },
      MuiDivider: {
        root: {
          backgroundColor: "#f3f3f3",
        },
      },
      MuiDialogTitle: {
        root: {
          paddingBottom: 0,
        },
      },
      MuiRadio: {
        colorSecondary: {
          "&$checked": {
            color: "#D52B1E",
          },
        },
        root: {
          padding: "4px",
          "& .MuiSvgIcon-root": {
            height: 22,
            width: 22,
          },
        },
      },
      MuiCheckbox: {
        root: {
          padding: "4px",
          "& .MuiSvgIcon-root": {
            height: 22,
            width: 22,
          },
        },
      },
      MuiTableSortLabel: {
        root: {
          verticalAlign: "unset",
        },
      },
      MuiAutocomplete: {
        input: {
          padding: "10px !important",
        },
        inputRoot: {
          paddingRight: direction === "ltr" ? 56 : "0px !important",
          paddingLeft: direction === "ltr" ? null : 56,
        },
        endAdornment: {
          right: direction === "ltr" ? 0 : null,
          left: direction === "ltr" ? null : 0,
        },
      },
      MuiTooltip: {
        tooltip: {
          fontSize: "1rem",
          padding: "8px",
        },
      },
      MuiSwitch: {
        root: {
          marginTop: "7px !important",
        },
      },
      MuiLinearProgress: {
        root: {
          marginBottom: "-3px",
          height: "3px",
        },
      },
      MuiPopover: {
        paper: {
          maxWidth: "400px",
        },
      },
      MuiSelect: {
        icon: {
          left: direction === "ltr" ? null : 0,
          right: direction === "ltr" ? 0 : null,
        },
        select: {
          paddingRight: direction === "ltr" ? "24px" : "10px !important",
          paddingLeft: direction === "ltr" ? 10 : "24px",
        },
      },
      MuiChip: {
        deleteIcon: {
          margin: direction === "ltr" ? "0 5px 0 -6px" : "0 -6px 0 5px",
        },
        icon: {
          marginLeft: direction === "ltr" ? "5px" : "-6px",
          marginRight: direction === "ltr" ? "-6px" : "5px",
        },
        label: {
          fontSize: "1rem",
        },
      },
      MuiFormHelperText: {
        root: {
          fontSize: "1rem",
          textAlign: direction === "ltr" ? "left" : "right",
        },
      },
      MuiSnackbarContent: {
        action: {
          marginLeft: direction === "ltr" ? "auto" : "-8px",
          marginRight: direction === "ltr" ? "-8px" : "auto",
          paddingLeft: direction === "ltr" ? "16px" : "0",
          paddingRight: direction === "ltr" ? "0" : "16px",
        },
      },
    },
  });
