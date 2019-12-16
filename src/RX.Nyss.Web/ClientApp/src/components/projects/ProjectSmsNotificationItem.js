import React, { useState } from 'react';
import { validators } from '../../utils/forms';
import TextActionInputField from '../forms/TextActionInputField';
import { useMount } from '../../utils/lifecycle';
import Grid from '@material-ui/core/Grid';
import DeleteIcon from '@material-ui/icons/Delete';

export const ProjectSmsNotificationItem = ({ itemKey, form, smsNotification, onRemove }) => {
  const [ready, setReady] = useState(false);

  useMount(() => {
    form.addField(`sms_notification_${itemKey}_id`, smsNotification.id);
    form.addField(`sms_notification_${itemKey}_phone_number`, smsNotification.phoneNumber, [validators.required, validators.maxLength(20), validators.phoneNumber]);

    setReady(true);

    return () => {
      form.removeField(`sms_notification_${itemKey}_id`);
      form.removeField(`sms_notification_${itemKey}_phone_number`);
    };
  });

  if (!ready) {
    return null;
  }

  return (
    <Grid item xs={12}>
      <TextActionInputField
        name={`sms_notification_${itemKey}_phone_number`}
        field={form.fields[`sms_notification_${itemKey}_phone_number`]}
        icon={<DeleteIcon />}
        onButtonClick={onRemove}
      />
    </Grid>
  );
}
