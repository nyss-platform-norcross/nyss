import styles from './ProjectAlertNotHandledRecipientItem.module.scss';
import Typography from "@material-ui/core/Typography";
import { useEffect, useState } from "react";
import EditIcon from '@material-ui/icons/Edit';
import Select from "@material-ui/core/Select";
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import Grid from "@material-ui/core/Grid";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import { stringKeys, strings } from "../../../strings";
import { useSelector } from "react-redux";

export const ProjectAlertNotHandledRecipientItem = ({ recipient, isAdministrator, getFormData, projectId, edit }) => {
  const [isEditing, setIsEditing] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);
  const users = useSelector(state => state.projectAlertNotHandledRecipients.users);
  const isSaving = useSelector(state => state.projectAlertNotHandledRecipients.saving);

  useEffect(() => {
    setSelectedUser(recipient);
  }, [recipient]);

  const openEdition = () => {
    getFormData(projectId);
    setIsEditing(true);
  }

  const handleRecipientChange = (change) => {
    const user = users.filter(u => u.userId === change.target.value)[0];
    setSelectedUser(user);
  }

  const onEdit = () => {
    edit(projectId, {
      userId: selectedUser.userId,
      organizationId: selectedUser.organizationId
    });
  }

  if (selectedUser == null) {
    return null;
  }

  return !isEditing ? (
    <Grid item className={styles.recipient} xs={12}>
      {isAdministrator && (
        <Typography variant="body1" className={styles.organizationField}>
          {recipient.organizationName}
        </Typography>
      )}

      <Typography variant="body1" className={styles.recipientName}>
        {recipient.name}
      </Typography>

      <EditIcon onClick={openEdition} />
    </Grid>
  ) : (
    <Grid item className={styles.recipient} xs={12}>
      {isAdministrator && (
        <Typography variant="body1" className={styles.organizationField}>
          {recipient.organizationName}
        </Typography>
      )}

      <Select
        className={styles.recipientNameSelect}
        value={selectedUser.userId}
        onChange={handleRecipientChange}
      >
        {users.map(u => (
          <MenuItem key={`recipient_user_${u.userId}`} value={u.userId}>
            {u.name}
          </MenuItem>
        ))}
      </Select>

      <Button onClick={() => setIsEditing(false)}>{strings(stringKeys.form.cancel)}</Button>
      <SubmitButton isFetching={isSaving} onClick={onEdit}>{strings(stringKeys.projectAlertNotHandledRecipient.update)}</SubmitButton>
    </Grid>
  )
}