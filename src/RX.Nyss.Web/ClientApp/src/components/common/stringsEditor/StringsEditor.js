import styles from "./StringsEditor.module.scss";

import React, { useState, Fragment } from 'react';
import { StringsEditorDialog } from './StringsEditorDialog'

export const StringsEditor = ({ stringKey }) => {
  const [open, setOpen] = useState(false);

  const onStringClick = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setOpen(true);
  }

  return (
    <Fragment>
      <span onClick={onStringClick} className={styles.stringKey} title={stringKey}>{stringKey}</span>

      {open &&
        <StringsEditorDialog stringKey={stringKey} close={(e) => { setOpen(false); }} />
      }
    </Fragment>
  );
}