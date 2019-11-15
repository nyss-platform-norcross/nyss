import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import Box from '@material-ui/core/Box';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import * as roles from '../../authentication/roles';

const NationalSocietyUsersAddExistingPageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      role: roles.Manager,   
      email: "",     
    };

    const validation = {
      email: [validators.required, validators.email, validators.maxLength(100)],
    };

    return createForm(fields, validation);
  });

  useMount(() => {
    props.openAddExisting(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.addExisting(props.nationalSocietyId, form.getValues());
  };

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.nationalSocietyUser.form.addExistingTitle)}</Typography>      

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

       <Form onSubmit={handleSubmit}>
        <Box mb={3}>
          <Typography variant="body1">{strings(stringKeys.nationalSocietyUser.form.addExistingDescription)}</Typography>
        </Box> 
        
         <Grid container spacing={3}>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.email)}
              name="email"
              field={form.fields.email}
              autoFocus
            />
          </Grid>

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
  error: state.nationalSocietyUsers.formError
});

const mapDispatchToProps = {
  openAddExisting: nationalSocietyUsersActions.openAddExisting.invoke,
  addExisting: nationalSocietyUsersActions.addExisting.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersAddExistingPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersAddExistingPageComponent)
);
