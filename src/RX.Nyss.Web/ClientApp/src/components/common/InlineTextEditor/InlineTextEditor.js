import styles from "./InlineTextEditor.module.scss"

import React, { useState, useRef } from 'react';
import { strings, stringKeys } from '../../../strings';
import Button from '@material-ui/core/Button';
import TextField from '@material-ui/core/TextField';
import InputAdornment from '@material-ui/core/InputAdornment';

export const InlineTextEditor = ({ initialValue, onSave, onClose, placeholder, autoFocus }) => {
  const [value, setValue] = useState(initialValue || "");
  const [isFocused, setIsFocused] = useState(false);

  const stopPropagation = e => e.stopPropagation();

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!value) {
      return;
    }

    onSave(value);
    setValue("");
  };

  const handleCancel = (e) => {
    e.stopPropagation();
    onClose();
  };

  const button = () => (
    <InputAdornment position="end">
      {onClose && <Button onClick={handleCancel}>{strings(stringKeys.form.cancel)}</Button>}
      <Button onClick={stopPropagation} onMouseDown={event => event.preventDefault()} type="submit" color="primary">{strings(stringKeys.form.inlineSave)}</Button>
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
  }

  const formRef = useRef(null);

  return <form onSubmit={handleSubmit} onFocus={() => setIsFocused(true)} onBlur={onBlur}>
    <TextField
      autoFocus={autoFocus}
      placeholder={placeholder}
      ref={formRef}
      error={isFocused && !value}
      className={styles.addText}
      onKeyDown={stopPropagation}
      value={value}
      onChange={e => setValue(e.target.value)}
      onClick={stopPropagation}
      InputProps={{
        endAdornment: isFocused ? button() : null
      }} />
  </form>;
}
