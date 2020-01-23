import styles from "./NationalSocietyStructureTree.module.scss"

import React, { Fragment, useState } from 'react';
import { strings, stringKeys } from '../../strings';
import TreeItem from '@material-ui/lab/TreeItem';
import Icon from "@material-ui/core/Icon";
import ConfirmationAction from "../common/confirmationAction/ConfirmationAction";
import { InlineTextEditor } from "../common/InlineTextEditor/InlineTextEditor";

export const StructureTreeItem = ({ itemKey, item, onEdit, onRemove, children, nationalSocietyIsArchived }) => {
  const [isEdited, setIsEdited] = useState(false);

  const handleEdit = (e) => {
    e.stopPropagation();
    setIsEdited(true);
  }

  const handleSave = (newName) => {
    onEdit(item.id, newName);
    setIsEdited(false);
  }

  const handleClose = () =>
    setIsEdited(false);

  const handleRemove = () =>
    onRemove(item.id);

  const treeItemContent = (label) => (
    <div className={styles.treeItemContent}>
      {isEdited && (
        <InlineTextEditor
          initialValue={item.name}
          onSave={handleSave}
          autoFocus
          onClose={handleClose} />
      )}

      {!isEdited && (
        <Fragment>
          <div>{label}</div>
          {!nationalSocietyIsArchived &&
            <Fragment>
              <Icon
                className={styles.itemAction}
                onClick={handleEdit}>
                edit
              </Icon>
              <ConfirmationAction
                confirmationText={strings(stringKeys.nationalSociety.structure.removalConfirmation)}
                onClick={handleRemove}>
                <Icon className={styles.itemAction}>delete_outline</Icon>
              </ConfirmationAction>
            </Fragment>
          }
        </Fragment>
      )}
    </div>
  );

  return (
    <TreeItem
      key={`${itemKey}_${item.id}`}
      nodeId={`${itemKey}_${item.id}`}
      label={treeItemContent(item.name)}>
      {children}
    </TreeItem>
  );
}
