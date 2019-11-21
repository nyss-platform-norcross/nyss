import styles from './MultiSelect.module.scss';

import React from "react";
import Select from 'react-select';
import Typography from '@material-ui/core/Typography';
import TextField from '@material-ui/core/TextField';
import Paper from '@material-ui/core/Paper';
import Chip from '@material-ui/core/Chip';
import MenuItem from '@material-ui/core/MenuItem';
import CancelIcon from '@material-ui/icons/Cancel';

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
  dropdownIndicator: (provided) => ({
    ...provided,
    cursor: 'pointer',
  })
}

export const MultiSelect = ({ name, error, label, value, defaultValue, options, onChange }) => {
  return (
    <Select
      defaultValue={defaultValue}
      isMulti
      name={name}
      options={options}
      inputId={name}
      value={value}
      onChange={onChange}
      placeholder={""}
      components={components}
      styles={customMultiselectStyle}
      TextFieldProps={{
        label: label,
        error: !!error,
        helperText: error,
        InputLabelProps: {
          htmlFor: { name },
          shrink: true
        },
      }}
    />
  );
};

function MultiValue(props) {
  return (
    <Chip
      tabIndex={-1}
      label={props.children}
      className={`${styles.chip} ${props.isFocused ? styles.chipFocused : ""}`}
      onDelete={props.removeProps.onClick}
      deleteIcon={<CancelIcon {...props.removeProps} />}
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
    <Paper
      square
      className={styles.paper}
      {...props.innerProps}>
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
    <Typography color="textSecondary"
      className={styles.placeholder}
      {...innerProps}>
      {children}
    </Typography>
  );
}

function SingleValue(props) {
  return (
    <Typography
      className={styles.singleValue}
      {...props.innerProps}>
      {props.children}
    </Typography>
  );
}

function ValueContainer(props) {
  return <div
    className={styles.valueContainer}
  >{props.children}</div>;
}
