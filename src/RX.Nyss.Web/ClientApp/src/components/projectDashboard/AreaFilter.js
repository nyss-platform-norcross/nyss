import React, { Fragment, useRef, useState } from 'react';
import { connect } from "react-redux";
import Menu from "@material-ui/core/Menu";

const AreaFilterComponent = ({ }) => {
  const [dropdownVisible, setDropdownVisible] = useState(false);
  const triggerRef = useRef(null);

  const handleDropdownClick = (e) => {
    setDropdownVisible(true);
  }

  const handleDropdownClose = (e) => {
    setDropdownVisible(false);
  };

  return (
    <Fragment>
      <div onClick={handleDropdownClick} ref={triggerRef}>
        Region: X
      </div>
      <Menu
        anchorEl={triggerRef.current}
        onClose={handleDropdownClose}
        open={dropdownVisible}
      >
        Hello
      </Menu>
    </Fragment>
  );
}


const mapStateToProps = state => ({
});

const mapDispatchToProps = {
};

export const AreaFilter = connect(mapStateToProps, mapDispatchToProps)(AreaFilterComponent);