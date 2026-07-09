import styles from "./StringsEditor.module.scss";

import React, { useState, Fragment } from "react";
import { StringsEditorDialog } from "./StringsEditorDialog";
import { EmailStringsEditorDialog } from "./EmailStringsEditorDialog";
import { SmsStringsEditorDialog } from "./SmsStringsEditorDialog";

export const StringsEditor = ({ stringKey, type, needsImprovement }) => {
  const [open, setOpen] = useState(false);

  const onStringClick = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setOpen(true);
  };

  return (
    <Fragment>
      <span
        onClick={onStringClick}
        className={styles.stringKey}
        title={stringKey}
      >{`${stringKey}${needsImprovement ? "*" : ""}`}</span>

      {open && type === "strings" && (
        <StringsEditorDialog
          stringKey={stringKey}
          close={(e) => {
            setOpen(false);
          }}
        />
      )}
      {open && type === "email" && (
        <EmailStringsEditorDialog
          stringKey={stringKey}
          close={(e) => {
            setOpen(false);
          }}
        />
      )}
      {open && type === "sms" && (
        <SmsStringsEditorDialog
          stringKey={stringKey}
          close={(e) => {
            setOpen(false);
          }}
        />
      )}
    </Fragment>
  );
};
