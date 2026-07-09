import React, { useState } from "react";
import styles from "./AlertEventExpandableText.module.scss";
import KeyboardArrowDownIcon from "@material-ui/icons/KeyboardArrowDown";
import KeyboardArrowUpIcon from "@material-ui/icons/KeyboardArrowUp";
import { TableRowAction } from "../../common/tableRowAction/TableRowAction";
import { TableRowActions } from "../../common/tableRowAction/TableRowActions";
import { IconButton } from "@material-ui/core";

export const AlertEventExpandableText = ({ text, maxLength }) => {
  const [isExpanded, setIsExpanded] = useState(false);

  const isTooLong = text && text.length > maxLength;

  const getTruncatedText = () => {
    return text.substring(0, maxLength - 3) + "...";
  };

  const renderShortText = () => {
    return <span>{text}</span>;
  };

  const getText = () => {
    if (!isExpanded) {
      return getTruncatedText();
    }
    return text;
  };

  const renderExpandableText = () => {
    return (
      <div className={styles.collapsibleContentWrapper}>
        <div>{getText()}</div>
        <TableRowActions className={styles.actionsMargin}>
          <TableRowAction
            aria-label="expand row"
            size="small"
            title={"Expand text"}
            onClick={() => setIsExpanded(true)}
            icon={<KeyboardArrowDownIcon />}
          ></TableRowAction>
        </TableRowActions>
      </div>
    );
  };

  const renderShrinkableText = () => {
    return (
      <div className={styles.collapsibleContentWrapper}>
        <div>{getText()}</div>
        <TableRowActions className={styles.actionsMargin}>
          <IconButton
            aria-label="shrink row"
            size="small"
            title={"Collapse text"}
            onClick={() => setIsExpanded(false)}
          >
            <KeyboardArrowUpIcon />
          </IconButton>
        </TableRowActions>
      </div>
    );
  };

  if (!isTooLong) {
    return renderShortText();
  }

  if (!isExpanded) {
    return renderExpandableText();
  } else {
    return renderShrinkableText();
  }
};
