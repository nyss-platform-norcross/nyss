import styles from './ProjectAlertNotHandledRecipientItem.module.scss';
import React, { useEffect, useState } from "react";
import EditIcon from '@material-ui/icons/Edit';
import { Select, MenuItem, Grid, Typography } from "@material-ui/core";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import { stringKeys, strings } from "../../../strings";
import { useSelector } from "react-redux";
import { Fragment } from 'react';
import CancelButton from "../../forms/cancelButton/CancelButton";

export const ProjectAlertNotHandledRecipientItem = ({ recipient, isAdministrator, getFormData, projectId, organizationId, edit, create }) => {
  const [isEditing, setIsEditing] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const users = useSelector(state => state.projectAlertNotHandledRecipients.users);
  const isSaving = useSelector(state => state.projectAlertNotHandledRecipients.saving);

  useEffect(() => {
    setSelectedUser(recipient);
  }, [recipient]);

  const openEdition = () => {
    getFormData(projectId);

    if (!!selectedUser.userId) {
      setIsEditing(true);
    } else {
      setIsCreating(true);
    }
  }

  const handleRecipientChange = (change) => {
    const user = users.filter(u => u.userId === change.target.value)[0];
    setSelectedUser(user);
  }

  const onEdit = () => {
    edit(projectId, {
      userId: selectedUser.userId,
      organizationId: recipient.organizationId,
    });
  }

  const onCreate = () => {
    create(projectId, {
      userId: selectedUser.userId,
      organizationId: recipient.organizationId,
    });
  }

  if (selectedUser == null) {
    return null;
  }

  return (
    <Grid item className={styles.recipient} xs={12}>
      {!(isEditing || isCreating) && (
        <Fragment>

          <Typography variant="body1" className={styles.recipientName}>
            {recipient.name}
          </Typography>

          {isAdministrator && (
            <Typography variant="body1" className={styles.organizationField}>
              {recipient.organizationName}
            </Typography>
          )}

          <EditIcon onClick={openEdition} />
        </Fragment>
      )}

      {isEditing && (
        <Fragment>

          <Select
            className={styles.recipientNameSelect}
            value={selectedUser.userId}
            onChange={handleRecipientChange}
          >
            {users
              .filter(u => u.organizationId === recipient.organizationId)
              .map(u => (
              <MenuItem key={`recipient_user_${u.userId}`} value={u.userId}>
                {u.name}
              </MenuItem>
            ))}
          </Select>

          {isAdministrator && (
            <Typography variant="body1" className={styles.organizationField}>
              {recipient.organizationName}
            </Typography>
          )}

          <CancelButton onClick={() => setIsEditing(false)}>{strings(stringKeys.form.cancel)}</CancelButton>
          <SubmitButton isFetching={isSaving} onClick={onEdit}>{strings(stringKeys.projectAlertNotHandledRecipient.update)}</SubmitButton>
        </Fragment>
      )}

      {isCreating && (
        <Fragment>

          <Select
            className={styles.recipientNameSelect}
            value={selectedUser.userId || ''}
            onChange={handleRecipientChange}
          >
            {users.map(u => (
              <MenuItem key={`recipient_user_${u.userId}`} value={u.userId}>
                {u.name}
              </MenuItem>
            ))}
          </Select>

          {isAdministrator && (
            <Typography variant="body1" className={styles.organizationField}>
              {recipient.organizationName}
            </Typography>
          )}

          <CancelButton onClick={() => setIsCreating(false)}>{strings(stringKeys.form.cancel)}</CancelButton>
          <SubmitButton isFetching={isSaving} onClick={onCreate}>{strings(stringKeys.projectAlertNotHandledRecipient.create)}</SubmitButton>
        </Fragment>
      )}
    </Grid>
  )
}