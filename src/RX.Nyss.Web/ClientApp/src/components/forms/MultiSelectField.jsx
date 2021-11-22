import PropTypes from "prop-types";
import { FormControl, InputLabel, Select } from "@material-ui/core";

const MultiSelectField = ({ name, label, value, controlProps, onChange, children, className, renderValues }) => {
  
  const renderSelectedValues = (selected) => 
    !!renderValues ? renderValues(selected) : selected.join(',');

  return (
    <FormControl>
      <InputLabel id={name} shrink>{label}</InputLabel>
      <Select
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
          anchorOrigin: {
            horizontal: 'left',
            vertical: 'bottom'
          },
          getContentAnchorEl: null
        }}>
        {children}
      </Select>
    </FormControl>
  );
};

MultiSelectField.propTypes = {
  label: PropTypes.string,
  controlProps: PropTypes.object,
  name: PropTypes.string,
  error: PropTypes.string,
  renderValues: PropTypes.func
};

export default MultiSelectField;
