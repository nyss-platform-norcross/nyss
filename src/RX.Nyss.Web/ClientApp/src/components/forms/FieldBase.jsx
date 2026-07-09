import React, { PureComponent } from "react";
import PropTypes from "prop-types";
import dayjs from "dayjs";

class FieldBase extends PureComponent {
  constructor(props) {
    super(props);

    this.state = {
      value: props.field.value === undefined ? "" : props.field.value,
      error: props.field.error,
    };

    this.subscription = props.field.subscribe(({ newValue, field }) => {
      this.setState({
        value: newValue,
        error: field.error,
      });
    });
  }

  setAutoCompleteValue = (value) =>
    value ? value.inputValue || value.title : "";

  handleChange = (e, val) => {
    // phone number component sends value where other elements send event
    const type =
      typeof e !== "string"
        ? this.getElementType((e.nativeEvent && e.nativeEvent.target) || e)
        : null;
    const value = this.getValue(type, e, val);

    this.setState({ value: value });
    this.props.field._customError = null;
    this.props.field.update(
      value,
      !this.props.field.touched && (type === "textbox" || type === "password"),
    );
    this.props.field.touched = true;

    if (this?.props?.subscribeOnChange != null) {
      this.props.subscribeOnChange(e, value);
    }
  };

  getValue = (type, e, val) => {
    if (!type) return `+${e}`; // phone number input custom change handling
    return type === "checkbox"
      ? e.target.checked
      : type === "date"
        ? dayjs(e)
        : this.setAutoCompleteValue(val) || e.target.value;
  };

  handleBlur = () => {};

  getElementType = (element) => {
    if (typeof element.toISOString === "function") {
      return "date";
    }

    if (element && element.type) {
      switch (element.type.toLowerCase()) {
        case "checkbox":
          return "checkbox";
        case "text":
          return "textbox";
        case "password":
          return "textbox";
        default:
      }
    }

    return element.tagName.toLowerCase();
  };

  componentWillUnmount() {
    this.subscription.unsubscribe();
  }

  render() {
    const { label, field, Component, ...rest } = this.props;

    return (
      <Component
        error={field.error}
        name={field.name}
        label={label}
        value={this.state.value}
        controlProps={{
          onChange: this.handleChange,
          onBlur: this.handleBlur,
        }}
        customProps={rest}
        {...rest}
      />
    );
  }
}

FieldBase.propTypes = {
  Component: PropTypes.func,
  field: PropTypes.shape({
    subscribe: PropTypes.func,
    update: PropTypes.func,
    value: PropTypes.any,
    name: PropTypes.string,
    error: PropTypes.any,
  }),
};

export const createFieldComponent = (Component) => (props) => (
  <FieldBase {...props} Component={Component} />
);

export default FieldBase;
