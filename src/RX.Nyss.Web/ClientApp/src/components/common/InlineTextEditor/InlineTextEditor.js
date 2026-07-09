import styles from "./InlineTextEditor.module.scss";

import React, { useState, useRef } from "react";
import { strings, stringKeys } from "../../../strings";
import { Button, TextField, InputAdornment } from "@material-ui/core";

export const InlineTextEditor = ({
  initialValue,
  onSave,
  onClose,
  placeholder,
  autoFocus,
  setIsModifying,
}) => {
  const [value, setValue] = useState(initialValue || "");
  const [isFocused, setIsFocused] = useState(false);

  const stopPropagation = (e) => e.stopPropagation();

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!value) {
      return;
    }

    onSave(value);
    setValue("");
  };

  const button = () => (
    <InputAdornment position="end">
      <Button
        onClick={stopPropagation}
        onMouseDown={(event) => event.preventDefault()}
        type="submit"
        color="primary"
      >
        {strings(stringKeys.common.buttons.update)}
      </Button>
    </InputAdornment>
  );

  const onBlur = (e) => {
    if (formRef.current.contains(e.relatedTarget)) {
      return;
    }

    setIsFocused(false);
    setValue("");

    if (onClose) {
      onClose();
    }

    if (setIsModifying) {
      setIsModifying(false);
    }
  };

  const formRef = useRef(null);

  return (
    <form
      onSubmit={handleSubmit}
      onFocus={() => setIsFocused(true)}
      onBlur={onBlur}
    >
      <TextField
        autoFocus={autoFocus}
        placeholder={placeholder}
        ref={formRef}
        error={isFocused && !value}
        className={styles.addText}
        onKeyDown={stopPropagation}
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onClick={stopPropagation}
        InputProps={{
          endAdornment: isFocused ? button() : null,
        }}
      />
    </form>
  );
};
