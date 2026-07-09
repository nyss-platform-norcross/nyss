import styles from "./ProjectAlertNotHandledRecipientItem.module.scss";
import React, { useEffect, useState } from "react";
import { Select, MenuItem, Grid, Typography, IconButton } from "@material-ui/core";
import DeleteIcon from '@material-ui/icons/Delete';
import { useSelector } from "react-redux";
import { strings, stringKeys } from "../../../strings";

export const ProjectAlertNotHandledRecipientItem = ({
  isAdministrator,
  getFormData,
  projectId,
  rtl,
  unhandledRecipient,
  unhandledRecipients,
  setUnhandledRecipients,
  setNewRecipient,
  isCreating,
  isEditing,
  error,
  setError,
  remove,
  hideDelete,
}) => {
  const [user, setUser] = useState(unhandledRecipient);
  const users = useSelector(
    (state) => state.projectAlertNotHandledRecipients.users,
  );

  useEffect(() => {
    getFormData(projectId);
  }, [projectId, getFormData]);

  const handleRecipientChange = (change) => {
    const user = users.filter((u) => u.userId === change.target.value)[0];
    setUser(user);
    setError(false);
    if (setNewRecipient) {
      setNewRecipient(user);
    } else {
      const removedRecipientIndex = unhandledRecipients.findIndex(
        (rec) => rec.userId === unhandledRecipient.userId,
      );
      let newRecipientList = [...unhandledRecipients];
      newRecipientList[removedRecipientIndex] = user;
      setUnhandledRecipients(newRecipientList);
    }
  };

  const handleRecipientDelete = () => {
    remove(projectId, user);
  }

  const recipientIds = unhandledRecipients?.map(
    (recipient) => recipient.userId,
  );
  const editRecipients = [
    ...users.filter((user) => !recipientIds.includes(user.userId)),
    user,
  ];
  const addRecipients = users.filter(
    (user) => !recipientIds.includes(user.userId),
  );
  const userList = setNewRecipient ? addRecipients : editRecipients;

  return (
    <Grid container item alignItems="center" style={{ marginTop: 20, width: "fit-content" }}>
      {(isEditing || isCreating) && (
        <Grid>
          <Select
            className={`${styles.recipientNameSelect} ${rtl ? styles.rtl : ""}`}
            value={user?.userId}
            onChange={handleRecipientChange}
            error={error}
            MenuProps={{
              PaperProps: {
                style: {
                  width: "90%",
                },
              },
            }}
          >
            {userList.map((u) => (
              <MenuItem style={{ whiteSpace: "normal" }} key={`recipient_user_${u.userId}`} value={u.userId}>
                {u.name}
              </MenuItem>
            ))}
          </Select>
          {error && (
            <Typography
              color="error"
              style={{ marginTop: 3 }}
              variant="subtitle2"
            >
              {strings(stringKeys.projectAlertNotHandledRecipient.error)}
            </Typography>
          )}
        </Grid>
      )}
      {!isEditing && !isCreating && (
        <Typography style={{ fontWeight: 700, width: 200, marginRight: 30 }}>
          {user.name}
        </Typography>
      )}
      {isAdministrator && (
        <Typography variant="body1" className={styles.organizationField}>
          {user?.organizationName}
        </Typography>
      )}
      {(!isEditing && !isCreating && !hideDelete && unhandledRecipients.length > 1) && (
        <IconButton color="primary" onClick={handleRecipientDelete}><DeleteIcon/></IconButton>
      )}
    </Grid>
  );
};
