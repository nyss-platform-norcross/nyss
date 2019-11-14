import React, { useState } from 'react';
import { validators } from '../../utils/forms';
import TextActionInputField from '../forms/TextActionInputField';
import { useMount } from '../../utils/lifecycle';
import Grid from '@material-ui/core/Grid';
import DeleteIcon from '@material-ui/icons/Delete';

export const ProjectNotificationItem = ({ itemKey, form, notification, onRemove }) => {
  const [ready, setReady] = useState(false);

  useMount(() => {
    form.addField(`notification_${itemKey}_id`, notification.id);
    form.addField(`notification_${itemKey}_email`, notification.email, [validators.required, validators.email]);

    setReady(true);

    return () => {
      form.removeField(`notification_${itemKey}_id`);
      form.removeField(`notification_${itemKey}_email`);
    };
  });

  if (!ready) {
    return null;
  }

  return (
    <Grid item xs={12}>
      <TextActionInputField
        name={`notification_${itemKey}_email`}
        field={form.fields[`notification_${itemKey}_email`]}
        icon={<DeleteIcon />}
        onButtonClick={onRemove}
      />
    </Grid>
  );
}
