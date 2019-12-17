import React, { useState } from 'react';
import { validators } from '../../utils/forms';
import TextActionInputField from '../forms/TextActionInputField';
import { useMount } from '../../utils/lifecycle';
import Grid from '@material-ui/core/Grid';
import DeleteIcon from '@material-ui/icons/Delete';

export const ProjectEmailNotificationItem = ({ itemKey, form, emailNotification, onRemove }) => {
  const [ready, setReady] = useState(false);

  useMount(() => {
    form.addField(`email_notification_${itemKey}_id`, emailNotification.id);
    form.addField(`email_notification_${itemKey}_email`, emailNotification.email, [validators.required, validators.maxLength(100), validators.email]);

    setReady(true);

    return () => {
      form.removeField(`email_notification_${itemKey}_id`);
      form.removeField(`email_notification_${itemKey}_email`);
    };
  });

  if (!ready) {
    return null;
  }

  return (
    <Grid item xs={12}>
      <TextActionInputField
        name={`email_notification_${itemKey}_email`}
        field={form.fields[`email_notification_${itemKey}_email`]}
        icon={<DeleteIcon />}
        onButtonClick={onRemove}
      />
    </Grid>
  );
}
