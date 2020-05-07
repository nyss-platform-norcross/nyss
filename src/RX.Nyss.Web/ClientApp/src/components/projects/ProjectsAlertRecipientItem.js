import styles from './ProjectsAlertRecipientItem.module.scss';
import React, { Fragment, useState } from 'react';
import { validators } from '../../utils/forms';
import TextInputField from '../forms/TextInputField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import Divider from '@material-ui/core/Divider';
import { IconButton, Icon } from '@material-ui/core';
import AutocompleteTextInputField from '../forms/AutocompleteTextInputField';

export const ProjectsAlertRecipientItem = ({ form, alertRecipient, alertRecipientNumber, onRemoveRecipient, organizations }) => {
  const [ready, setReady] = useState(false);

  useMount(() => {
    form.addField(`alertRecipientRole${alertRecipientNumber}`, alertRecipient.role, [validators.required, validators.maxLength(100)]);
    form.addField(`alertRecipientOrganization${alertRecipientNumber}`, alertRecipient.organization, [validators.required, validators.maxLength(100)]);
    form.addField(`alertRecipientEmail${alertRecipientNumber}`, alertRecipient.email, [validators.emailWhen(_ => _[`alertRecipientPhone${alertRecipientNumber}`] === ''), validators.requiredWhen(_ => _[`alertRecipientPhone${alertRecipientNumber}`] === ''), validators.maxLength(100)]);
    form.addField(`alertRecipientPhone${alertRecipientNumber}`, alertRecipient.phoneNumber, [validators.requiredWhen(_ => _[`alertRecipientEmail${alertRecipientNumber}`] === ''), validators.maxLength(20), validators.phoneNumber]);

    setReady(true);

    return () => {
      form.removeField(`alertRecipientRole${alertRecipientNumber}`);
      form.removeField(`alertRecipientOrganization${alertRecipientNumber}`);
      form.removeField(`alertRecipientEmail${alertRecipientNumber}`);
      form.removeField(`alertRecipientPhone${alertRecipientNumber}`);
    };
  })

  if (!ready) {
    return null;
  }

  return (
    <Fragment key={`alertRecipient${alertRecipient}`}>
      <Grid container spacing={1}>

        <Grid item xs={12} sm={6} lg={2}>
          <TextInputField
            label={strings(stringKeys.project.form.alertNotificationsRole)}
            name="role"
            field={form.fields[`alertRecipientRole${alertRecipientNumber}`]}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <AutocompleteTextInputField
            id="autocompleteOrganization"
            label={strings(stringKeys.project.form.alertNotificationsOrganization)}
            name="organization"
            freeSolo
            options={organizations}
            defaultValue={alertRecipient.organization}
            field={form.fields[`alertRecipientOrganization${alertRecipientNumber}`]}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <TextInputField
            label={strings(stringKeys.project.form.alertNotificationsEmail)}
            name="email"
            field={form.fields[`alertRecipientEmail${alertRecipientNumber}`]}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={3}>
          <TextInputField
            label={strings(stringKeys.project.form.alertNotificationsPhoneNumber)}
            name="phoneNumber"
            field={form.fields[`alertRecipientPhone${alertRecipientNumber}`]}
          />
        </Grid>
        <Grid item xs={12} sm={6} lg={1} className={styles.removeButtonContainer}>
          <IconButton onClick={() => onRemoveRecipient(alertRecipient)}>
            <Icon>delete</Icon>
          </IconButton>
        </Grid>

        <Grid item xs={12}>
          <Divider />
        </Grid>
      </Grid>
    </Fragment>
  );
}
