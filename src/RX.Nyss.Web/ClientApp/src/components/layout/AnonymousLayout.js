import React from "react";
import { BaseLayout } from "./BaseLayout";

import styles from "./AnonymousLayout.module.scss";
import { MessagePopup } from "./MessagePopup";

export const AnonymousLayout = ({ children }) => {
  return (
    <BaseLayout>
      <div className={styles.anonymousLayout}>
        <div className={styles.content}>{children}</div>
      </div>
      <MessagePopup />
    </BaseLayout>
  );
};
