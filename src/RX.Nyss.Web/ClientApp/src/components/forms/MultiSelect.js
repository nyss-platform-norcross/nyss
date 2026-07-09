import styles from "./MultiSelect.module.scss";

import React from "react";
import Select from "react-select";
import {
  TextField,
  Typography,
  Paper,
  Chip,
  MenuItem,
} from "@material-ui/core";
import CancelIcon from "@material-ui/icons/Cancel";

const components = {
  Control,
  Menu,
  MultiValue,
  NoOptionsMessage,
  Option,
  Placeholder,
  SingleValue,
  ValueContainer,
};

const customMultiselectStyle = {
  clearIndicator: (provided) => ({
    ...provided,
    cursor: "pointer",
  }),
  dropdownIndicator: (provided) => ({
    ...provided,
    cursor: "pointer",
  }),
};

export const MultiSelect = ({
  name,
  error,
  label,
  value,
  defaultValue,
  options,
  onChange,
  rtl,
  isClearable,
  ...restProps
}) => {
  return (
    <Select
      {...restProps}
      defaultValue={defaultValue}
      isMulti
      isClearable={isClearable}
      name={name}
      options={options}
      inputId={name}
      value={value}
      onChange={onChange}
      placeholder={""}
      components={components}
      style={customMultiselectStyle}
      isRtl={rtl}
      TextFieldProps={{
        label: label,
        error: !!error,
        helperText: error,
        InputLabelProps: {
          htmlFor: { name },
          shrink: true,
        },
      }}
    />
  );
};

function MultiValue(props) {
  const isActivityReport =
    !!props.data.data && props.data.data.healthRiskType === "Activity";

  return (
    <Chip
      tabIndex={-1}
      label={props.children}
      className={`${styles.chip} ${props.isFocused ? styles.chipFocused : ""}`}
      onDelete={!isActivityReport ? props.removeProps.onClick : null}
      deleteIcon={
        !isActivityReport ? <CancelIcon {...props.removeProps} /> : null
      }
      classes={{label: styles.chipLabel}}
    />
  );
}

function NoOptionsMessage(props) {
  return (
    <Typography
      color="textSecondary"
      className={styles.noOptionsMessage}
      {...props.innerProps}
    >
      {props.children}
    </Typography>
  );
}

function Menu(props) {
  return (
    <Paper square className={styles.paper} {...props.innerProps}>
      {props.children}
    </Paper>
  );
}

function inputComponent({ inputRef, ...props }) {
  return <div ref={inputRef} {...props} />;
}

function Control(props) {
  const {
    children,
    innerProps,
    innerRef,
    selectProps: { TextFieldProps },
  } = props;

  return (
    <TextField
      fullWidth
      InputProps={{
        inputComponent,
        inputProps: {
          className: styles.input,
          ref: innerRef,
          children,
          ...innerProps,
        },
      }}
      {...TextFieldProps}
    />
  );
}

function Option(props) {
  return (
    <MenuItem
      ref={props.innerRef}
      selected={props.isFocused}
      component="div"
      style={{
        fontWeight: props.isSelected ? 500 : 400,
      }}
      {...props.innerProps}
    >
      {props.children}
    </MenuItem>
  );
}

function Placeholder(props) {
  const { innerProps = {}, children } = props;
  return (
    <Typography
      color="textSecondary"
      className={styles.placeholder}
      {...innerProps}
    >
      {children}
    </Typography>
  );
}

function SingleValue(props) {
  return (
    <Typography className={styles.singleValue} {...props.innerProps}>
      {props.children}
    </Typography>
  );
}

function ValueContainer(props) {
  return <div className={styles.valueContainer}>{props.children}</div>;
}
