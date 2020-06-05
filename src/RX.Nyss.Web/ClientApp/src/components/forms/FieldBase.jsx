import React, { PureComponent } from "react";
import PropTypes from "prop-types";
import dayjs from "dayjs";

class FieldBase extends PureComponent {
  constructor(props) {
    super(props);

    this.state = {
      value: props.field.value === undefined ? "" : props.field.value,
      error: props.field.error
    };

    this.subscription = props.field.subscribe(({ newValue, field }) => {
      this.setState({
        value: newValue,
        error: field.error
      });
    });
  };

  setAutoCompleteValue = (value) =>
    value ? (value.inputValue || value.title) : '';

  handleChange = (e, val) => {
    const type = this.getElementType((e.nativeEvent && e.nativeEvent.target) || e);
    const value = type === "checkbox"
      ? e.target.checked
      : (type === "date"
        ? dayjs(e)
        : this.setAutoCompleteValue(val) || e.target.value);

    this.setState({ value: value });
    this.props.field._customError = null;
    this.props.field.update(value, !this.props.field.touched && (type === "textbox" || type === "password"));
  }

  handleBlur = () => {
    this.props.field.touched = true;
    this.props.field.update(this.props.field.value);
  }

  getElementType = (element) => {
    if (typeof element.toISOString === "function") {
      return "date";
    }

    if (element && element.type) {
      switch (element.type.toLowerCase()) {
        case "checkbox": return "checkbox";
        case "text": return "textbox";
        case "password": return "textbox";
        default:
      }
    }

    return element.tagName.toLowerCase();
  }

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
          onBlur: this.handleBlur
        }}
        customProps={rest}
        {...rest}
      />
    );
  }
};

FieldBase.propTypes = {
  Component: PropTypes.func,
  field: PropTypes.shape({
    subscribe: PropTypes.func,
    update: PropTypes.func,
    value: PropTypes.any,
    name: PropTypes.string,
    error: PropTypes.any
  })
};

export const createFieldComponent = (Component) => (props) => (
  <FieldBase
    {...props}
    Component={Component}
  />);

export default FieldBase;
