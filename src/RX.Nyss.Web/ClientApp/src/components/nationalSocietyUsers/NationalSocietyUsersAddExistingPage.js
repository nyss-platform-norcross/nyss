import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import Box from '@material-ui/core/Box';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { ValidationMessage } from '../forms/ValidationMessage';
import SelectField from '../forms/SelectField';
import { MenuItem } from '@material-ui/core';

const NationalSocietyUsersAddExistingPageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      email: "",
      modemId: ""
    };

    const validation = {
      email: [validators.required, validators.email, validators.maxLength(100)],
      modemId: [validators.requiredWhen(_ => canSelectModem)]
    };

    return createForm(fields, validation);
  });

  const canSelectModem = props.modems != null && props.modems.length > 0;

  useCustomErrors(form, props.error);

  useMount(() => {
    props.openAddExisting(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.addExisting(props.nationalSocietyId, {
      email: values.email,
      modemId: !!values.modemId ? parseInt(values.modemId) : null
    });
  };

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Box mb={3}>
          <Typography variant="body1">{strings(stringKeys.nationalSocietyUser.form.addExistingDescription)}</Typography>
        </Box>

        <Grid container spacing={2}>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.email)}
              name="email"
              field={form.fields.email}
              autoFocus
              inputMode={"email"}
            />
          </Grid>

          {canSelectModem && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.modem)}
                field={form.fields.modemId}
                name="modemId"
              >
                {props.modems.map(modem => (
                  <MenuItem key={`modemId_${modem.id}`} value={modem.id.toString()}>
                    {modem.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.nationalSocietyUser.form.addExisting)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

NationalSocietyUsersAddExistingPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError,
  modems: state.nationalSocietyUsers.formModems
});

const mapDispatchToProps = {
  openAddExisting: nationalSocietyUsersActions.openAddExisting.invoke,
  addExisting: nationalSocietyUsersActions.addExisting.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersAddExistingPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersAddExistingPageComponent)
);
