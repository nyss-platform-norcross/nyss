import PropTypes from "prop-types";
import { FormControl, InputLabel, Select } from "@material-ui/core";

const MultiSelectField = ({
  name,
  label,
  value,
  onChange,
  children,
  className,
  renderValues,
  rtl,
  isOpen,
  setIsOpen,
}) => {
  const renderSelectedValues = (selected) =>
    !!renderValues ? renderValues(selected) : selected.join(",");

  return (
    <FormControl>
      <InputLabel id={name} shrink>
        {label}
      </InputLabel>
      <Select
        open={isOpen}
        onOpen={() => setIsOpen(true)}
        onClose={() => setIsOpen(false)}
        multiple
        displayEmpty
        name={name}
        id={name}
        labelId={name}
        value={value}
        onChange={onChange}
        renderValue={renderSelectedValues}
        className={className}
        MenuProps={{
          PaperProps: {
            style: { maxHeight: "fit-content" },
          },
          anchorOrigin: {
            horizontal: rtl ? "right" : "left",
            vertical: "bottom",
          },
          transformOrigin: {
            horizontal: rtl ? "right" : "left",
            vertical: "top",
          },
          getContentAnchorEl: null,
        }}
      >
        {children}
      </Select>
    </FormControl>
  );
};

MultiSelectField.propTypes = {
  label: PropTypes.string,
  name: PropTypes.string,
  error: PropTypes.string,
  renderValues: PropTypes.func,
};

export default MultiSelectField;
